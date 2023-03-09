using ClangSharp.Interop;
using System.Collections.Immutable;

namespace MochaTool.InteropGen;

public static class Parser
{
	/// <summary>
	/// Cached launch arguments so that we don't have to regenerate them every time
	/// </summary>
	private static string[] s_launchArgs = GetLaunchArgs();
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

	public unsafe static List<IUnit> GetUnits( string path )
	{
		List<IUnit> units = new();

		using var index = CXIndex.Create();
		using var unit = CXTranslationUnit.Parse( index, path, s_launchArgs, ReadOnlySpan<CXUnsavedFile>.Empty, CXTranslationUnit_Flags.CXTranslationUnit_None );

		for ( int i = 0; i < unit.NumDiagnostics; ++i )
		{
			var diagnostics = unit.GetDiagnostic( (uint)i );
			Console.WriteLine( $"{diagnostics.Format( CXDiagnostic.DefaultDisplayOptions )}" );
		}

		var cursor = unit.Cursor;

		CXChildVisitResult cursorVisitor( CXCursor cursor, CXCursor parent, void* data )
		{
			if ( !cursor.Location.IsFromMainFile )
				return CXChildVisitResult.CXChildVisit_Continue;

			bool HasGenerateBindingsAttribute()
			{
				if ( !cursor.HasAttrs )
					return false;

				var attr = cursor.GetAttr( 0 );
				if ( attr.Spelling.CString != "generate_bindings" )
					return false;

				return true;
			}

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
					{
						if ( !HasGenerateBindingsAttribute() )
							return CXChildVisitResult.CXChildVisit_Continue;

						if ( cursor.CXXAccessSpecifier != CX_CXXAccessSpecifier.CX_CXXPublic && cursor.Kind != CXCursorKind.CXCursor_FunctionDecl )
							break;

						var ownerName = cursor.LexicalParent.Spelling.ToString();
						var owner = units.FirstOrDefault( x => x.Name == ownerName );
						if ( owner is null )
						{
							Console.WriteLine( $"No unit with name \"{ownerName}\"" );
							break;
						}

						var name = cursor.Spelling.ToString();
						var returnType = cursor.ReturnType.Spelling.ToString();
						var isStatic = cursor.IsStatic;
						var isConstructor = false;

						var parametersBuilder = ImmutableArray.CreateBuilder<Variable>();

						CXChildVisitResult methodChildVisitor( CXCursor cursor, CXCursor parent, void* data )
						{
							if ( cursor.Kind == CXCursorKind.CXCursor_ParmDecl )
							{
								var type = cursor.Type.ToString();
								var name = cursor.Spelling.ToString();

								parametersBuilder.Add( new Variable( name, type ) );
							}

							return CXChildVisitResult.CXChildVisit_Recurse;
						}

						cursor.VisitChildren( methodChildVisitor, default );

						if ( cursor.Kind == CXCursorKind.CXCursor_Constructor )
						{
							// Constructor specific stuff here
							name = "Ctor";
							returnType = $"{owner.Name}*";
							isConstructor = true;
						}
						Method method;
						if ( isConstructor )
							method = Method.NewConstructor( name, returnType, parametersBuilder.ToImmutable() );
						else
							method = Method.NewMethod( name, returnType, isStatic, parametersBuilder.ToImmutable() );

						var newOwner = owner.WithMethods( owner.Methods.Add( method ) );
						units.Remove( owner );
						units.Add( newOwner );

						break;
					}

				//
				// Field
				//
				case CXCursorKind.CXCursor_FieldDecl:
					{
						if ( !HasGenerateBindingsAttribute() )
							return CXChildVisitResult.CXChildVisit_Continue;

						var ownerName = cursor.LexicalParent.Spelling.ToString();
						var owner = units.FirstOrDefault( x => x.Name == ownerName );

						if ( owner is null )
							break;

						var newOwner = owner.WithFields( owner.Fields.Add( new Variable( cursor.Spelling.ToString(), cursor.Type.ToString() ) ) );
						units.Remove( owner );
						units.Add( newOwner );
						break;
					}

				default:
					break;
			}

			return CXChildVisitResult.CXChildVisit_Recurse;
		}

		cursor.VisitChildren( cursorVisitor, default );

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

		//
		// Post-processing
		//
		//foreach ( var o in units )
		//{
		//	// Create a default constructor if one wasn't already defined
		//	if ( !o.Methods.Any( x => x.IsConstructor ) && o is not Class { IsNamespace: true } )
		//	{
		//		Console.WriteLine( $"Creating default ctor for {o.Name}" );
		//		o.Methods.Add( new Method( "Ctor", $"{o.Name}*" )
		//		{
		//			IsConstructor = true
		//		} );
		//	}
		//}

		return units;
	}
}
