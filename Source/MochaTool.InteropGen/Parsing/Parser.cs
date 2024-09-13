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
	/// Parses a header file and returns all of the <see cref="IContainerUnit"/>s contained inside.
	/// </summary>
	/// <param name="path">The absolute path to the header file to parse.</param>
	/// <returns>All of the <see cref="IContainerUnit"/>s contained inside the header file.</returns>
	internal unsafe static IEnumerable<IContainerUnit> GetUnits( string path )
	{
		using var _time = new StopwatchLog( $"Parse {path}" );
		var units = new List<IContainerUnit>();

		using var index = CXIndex.Create();
		using var unit = CXTranslationUnit.Parse( index, path, s_launchArgs, ReadOnlySpan<CXUnsavedFile>.Empty, CXTranslationUnit_Flags.CXTranslationUnit_SkipFunctionBodies );

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

		ContainerBuilder? currentContainer = null;

		// Visits all immediate members inside of a class/struct/namespace declaration.
		CXChildVisitResult cursorMemberVisitor( CXCursor cursor, CXCursor parent, void* data )
		{
			if ( !cursor.Location.IsFromMainFile )
				return CXChildVisitResult.CXChildVisit_Continue;

			switch ( cursor.Kind )
			{
				//
				// Methods
				//
				case CXCursorKind.CXCursor_Constructor:
				case CXCursorKind.CXCursor_CXXMethod:
				case CXCursorKind.CXCursor_FunctionDecl:
					return VisitMethod( cursor, currentContainer );

				//
				// Field
				//
				case CXCursorKind.CXCursor_FieldDecl:
					return VisitField( cursor, currentContainer );
			}

			return CXChildVisitResult.CXChildVisit_Continue;
		}

		// Visits all elements within the translation unit.
		CXChildVisitResult cursorContainerVisitor( CXCursor cursor, CXCursor parent, void* data )
		{
			if ( !cursor.Location.IsFromMainFile )
				return CXChildVisitResult.CXChildVisit_Continue;

			var containerType = cursor.Kind switch
			{
				CXCursorKind.CXCursor_ClassDecl => ContainerType.Class,
				CXCursorKind.CXCursor_StructDecl => ContainerType.Struct,
				CXCursorKind.CXCursor_Namespace => ContainerType.Namespace,
				_ => ContainerType.Invalid
			};

			// Bail from recursing through if it's not an item we care about.
			if ( containerType == ContainerType.Invalid )
				return CXChildVisitResult.CXChildVisit_Continue;

			currentContainer = new ContainerBuilder( containerType, cursor.Spelling.ToString() );
			cursor.VisitChildren( cursorMemberVisitor, default );

			if ( !currentContainer.IsEmpty )
				units.Add( currentContainer.Build() );

			// Recurse through nested classes and structs.
			return CXChildVisitResult.CXChildVisit_Recurse;
		}

		unit.Cursor.VisitChildren( cursorContainerVisitor, default );
		return units;
	}

	/// <summary>
	/// The visitor method for walking a method declaration.
	/// </summary>
	/// <param name="cursor">The cursor that is traversing the method.</param>
	/// <param name="currentContainer">The current C++ container being parsed.</param>
	/// <returns>The next action the cursor should take in traversal.</returns>
	private static unsafe CXChildVisitResult VisitMethod( in CXCursor cursor, ContainerBuilder? currentContainer )
	{
		// Early bails.
		if ( currentContainer is null )
			return CXChildVisitResult.CXChildVisit_Continue;
		if ( !cursor.HasGenerateBindingsAttribute() )
			return CXChildVisitResult.CXChildVisit_Continue;
		if ( cursor.CXXAccessSpecifier != CX_CXXAccessSpecifier.CX_CXXPublic && cursor.Kind != CXCursorKind.CXCursor_FunctionDecl )
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
			returnType = currentContainer.Name + '*';
			isStatic = false;
			isConstructor = true;
			isDestructor = false;
		}
		// We're traversing a destructor.
		else if ( cursor.Kind == CXCursorKind.CXCursor_Destructor )
		{
			name = "DeCtor";
			returnType = '~' + currentContainer.Name;
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
		currentContainer.AddMethod( method );

		return CXChildVisitResult.CXChildVisit_Continue;
	}

	/// <summary>
	/// The visitor method for walking a field declaration.
	/// </summary>
	/// <param name="cursor">The cursor that is traversing the method.</param>
	/// <param name="currentContainer">The current C++ container being parsed.</param>
	/// <returns>The next action the cursor should take in traversal.</returns>
	private static CXChildVisitResult VisitField( in CXCursor cursor, ContainerBuilder? currentContainer )
	{
		// Early bail.
		if ( currentContainer is null )
			return CXChildVisitResult.CXChildVisit_Continue;
		if ( !cursor.HasGenerateBindingsAttribute() )
			return CXChildVisitResult.CXChildVisit_Continue;

		// Update owner with new field.
		currentContainer.AddField( new Variable( cursor.Spelling.ToString(), cursor.Type.ToString() ) );

		return CXChildVisitResult.CXChildVisit_Recurse;
	}

	/// <summary>
	/// Returns a compiled array of launch arguments to pass to the C++ parser.
	/// </summary>
	/// <returns>A compiled array of launch arguments to pass to the C++ parser.</returns>
	private static string[] GetLaunchArgs()
	{
		// Generate includes from vcxproj
		var includeDirs = VcxprojParser.ParseIncludes( Path.Combine( "..", "Mocha.Host", "Mocha.Host.vcxproj" ) );

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
