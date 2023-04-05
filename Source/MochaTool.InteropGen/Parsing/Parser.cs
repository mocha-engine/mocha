using ClangSharp.Interop;
using Microsoft.Extensions.Logging;
using MochaTool.InteropGen.Extensions;
using System.Collections.Immutable;

namespace MochaTool.InteropGen.Parsing;

/// <summary>
/// Contains all parsing functionality for C++ header files.
/// </summary>
internal static class Parser
{
	/// <summary>
	/// Cached launch arguments so that we don't have to regenerate them every time
	/// </summary>
	private static readonly string[] s_launchArgs = GetLaunchArgs();

	/// <summary>
	/// Parses a header file and returns all of the <see cref="IUnit"/>s contained inside.
	/// </summary>
	/// <param name="path">The absolute path to the header file to parse.</param>
	/// <returns>All of the <see cref="IUnit"/>s contained inside the header file.</returns>
	internal unsafe static IEnumerable<IUnit> GetUnits( string path )
	{
		var units = new List<IUnit>();

		using var index = CXIndex.Create();
		using var unit = CXTranslationUnit.Parse( index, path, s_launchArgs, ReadOnlySpan<CXUnsavedFile>.Empty, CXTranslationUnit_Flags.CXTranslationUnit_None );

		// Only start walking diagnostics if logging is enabled to the minimum level.
		if ( Log.IsEnabled( LogLevel.Warning ) )
		{
			for ( var i = 0; i < unit.NumDiagnostics; i++ )
			{
				var diagnostics = unit.GetDiagnostic( (uint)i );
				switch ( diagnostics.Severity )
				{
					case CXDiagnosticSeverity.CXDiagnostic_Fatal:
						Log.FatalDiagnostic( diagnostics.Format( CXDiagnostic.DefaultDisplayOptions ).CString );
						break;
					case CXDiagnosticSeverity.CXDiagnostic_Error:
						Log.ErrorDiagnostic( diagnostics.Format( CXDiagnostic.DefaultDisplayOptions ).CString );
						break;
					case CXDiagnosticSeverity.CXDiagnostic_Warning:
						Log.WarnDiagnostic( diagnostics.Format( CXDiagnostic.DefaultDisplayOptions ).CString );
						break;
				}
			}
		}

		CXChildVisitResult cursorVisitor( CXCursor cursor, CXCursor parent, void* data )
		{
			if ( !cursor.Location.IsFromMainFile )
				return CXChildVisitResult.CXChildVisit_Continue;

			switch ( cursor.Kind )
			{
				//
				// Struct / class / namespace
				//
				case CXCursorKind.CXCursor_ClassDecl:
					units.Add( Class.NewClass( cursor.Spelling.ToString(), ImmutableArray<Variable>.Empty, ImmutableArray<Method>.Empty ) );
					break;
				case CXCursorKind.CXCursor_StructDecl:
					units.Add( Struct.NewStructure( cursor.Spelling.ToString(), ImmutableArray<Variable>.Empty, ImmutableArray<Method>.Empty ) );
					break;
				case CXCursorKind.CXCursor_Namespace:
					units.Add( Class.NewNamespace( cursor.Spelling.ToString(), ImmutableArray<Variable>.Empty, ImmutableArray<Method>.Empty ) );
					break;

				//
				// Methods
				//
				case CXCursorKind.CXCursor_Constructor:
				case CXCursorKind.CXCursor_CXXMethod:
				case CXCursorKind.CXCursor_FunctionDecl:
					return VisitMethod( cursor, units );

				//
				// Field
				//
				case CXCursorKind.CXCursor_FieldDecl:
					return VisitField( cursor, units );
			}

			return CXChildVisitResult.CXChildVisit_Recurse;
		}

		unit.Cursor.VisitChildren( cursorVisitor, default );

		//
		// Remove all items with duplicate names
		//
		for ( int i = 0; i < units.Count; i++ )
		{
			var item = units[i];
			item = item.WithFields( item.Fields.GroupBy( x => x.Name ).Select( x => x.First() ).ToImmutableArray() )
				.WithMethods( item.Methods.GroupBy( x => x.Name ).Select( x => x.First() ).ToImmutableArray() );

			units[i] = item;
		}

		//
		// Remove any units that have no methods or fields
		//
		units = units.Where( x => x.Methods.Length > 0 || x.Fields.Length > 0 ).ToList();

		return units;
	}

	/// <summary>
	/// The visitor method for walking a method declaration.
	/// </summary>
	/// <param name="cursor">The cursor that is traversing the method.</param>
	/// <param name="units">The <see cref="IUnit"/> collection to fetch method owners from.</param>
	/// <returns>The next action the cursor should take in traversal.</returns>
	private static unsafe CXChildVisitResult VisitMethod( in CXCursor cursor, ICollection<IUnit> units )
	{
		// Early bails.
		if ( !cursor.HasGenerateBindingsAttribute() )
			return CXChildVisitResult.CXChildVisit_Continue;
		if ( cursor.CXXAccessSpecifier != CX_CXXAccessSpecifier.CX_CXXPublic && cursor.Kind != CXCursorKind.CXCursor_FunctionDecl )
			return CXChildVisitResult.CXChildVisit_Continue;

		// Verify that the method has an owner.
		var ownerName = cursor.LexicalParent.Spelling.ToString();
		var owner = units.FirstOrDefault( x => x.Name == ownerName );
		if ( owner is null )
			return CXChildVisitResult.CXChildVisit_Continue;

		string name;
		string returnType;
		bool isStatic;
		bool isConstructor;
		bool isDestructor;

		var parametersBuilder = ImmutableArray.CreateBuilder<Variable>();
		// We're traversing a constructor.
		if ( cursor.Kind == CXCursorKind.CXCursor_Constructor )
		{
			name = "Ctor";
			returnType = owner.Name + '*';
			isStatic = false;
			isConstructor = true;
			isDestructor = false;
		}
		// We're traversing a destructor.
		else if ( cursor.Kind == CXCursorKind.CXCursor_Destructor )
		{
			name = "DeCtor";
			returnType = '~' + owner.Name;
			isStatic = false;
			isConstructor = false;
			isDestructor = true;
		}
		// We're traversing a standard method.
		else
		{
			name = cursor.Spelling.ToString();
			returnType = cursor.ReturnType.Spelling.ToString();
			isStatic = cursor.IsStatic;
			isConstructor = false;
			isDestructor = false;
		}

		// Visitor for parameter delcarations.
		CXChildVisitResult methodChildVisitor( CXCursor cursor, CXCursor parent, void* data )
		{
			if ( cursor.Kind != CXCursorKind.CXCursor_ParmDecl )
				return CXChildVisitResult.CXChildVisit_Continue;

			var name = cursor.Spelling.ToString();
			var type = cursor.Type.ToString();

			parametersBuilder.Add( new Variable( name, type ) );

			return CXChildVisitResult.CXChildVisit_Recurse;
		}

		cursor.VisitChildren( methodChildVisitor, default );

		// Construct the method.
		Method method;
		if ( isConstructor )
			method = Method.NewConstructor( name, returnType, parametersBuilder.ToImmutable() );
		else if ( isDestructor )
			method = Method.NewDestructor( name, returnType, parametersBuilder.ToImmutable() );
		else
			method = Method.NewMethod( name, returnType, isStatic, parametersBuilder.ToImmutable() );

		// Update owner with new method.
		var newOwner = owner.WithMethods( owner.Methods.Add( method ) );
		units.Remove( owner );
		units.Add( newOwner );

		return CXChildVisitResult.CXChildVisit_Continue;
	}

	/// <summary>
	/// The visitor method for walking a field declaration.
	/// </summary>
	/// <param name="cursor">The cursor that is traversing the method.</param>
	/// <param name="units">The <see cref="IUnit"/> collection to fetch method owners from.</param>
	/// <returns>The next action the cursor should take in traversal.</returns>
	private static CXChildVisitResult VisitField( in CXCursor cursor, ICollection<IUnit> units )
	{
		// Early bail.
		if ( !cursor.HasGenerateBindingsAttribute() )
			return CXChildVisitResult.CXChildVisit_Continue;

		// Verify that the field has an owner.
		var ownerName = cursor.LexicalParent.Spelling.ToString();
		var owner = units.FirstOrDefault( x => x.Name == ownerName );
		if ( owner is null )
			return CXChildVisitResult.CXChildVisit_Recurse;

		// Update owner with new field.
		var newOwner = owner.WithFields( owner.Fields.Add( new Variable( cursor.Spelling.ToString(), cursor.Type.ToString() ) ) );
		units.Remove( owner );
		units.Add( newOwner );

		return CXChildVisitResult.CXChildVisit_Recurse;
	}

	/// <summary>
	/// Returns a compiled array of launch arguments to pass to the C++ parser.
	/// </summary>
	/// <returns>A compiled array of launch arguments to pass to the C++ parser.</returns>
	private static string[] GetLaunchArgs()
	{
		// Generate includes from vcxproj
		var includeDirs = VcxprojParser.ParseIncludes( "../Mocha.Host/Mocha.Host.vcxproj" );

		var args = new List<string>
		{
			"-x",
			"c++",
			"-fparse-all-comments",
			"-std=c++20",
			"-DVK_NO_PROTOTYPES",
			"-DNOMINMAX",
			"-DVK_USE_PLATFORM_WIN32_KHR"
		};

		args.AddRange( includeDirs.Select( x => "-I" + x ) );

		return args.ToArray();
	}
}
