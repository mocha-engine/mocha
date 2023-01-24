using ClangSharp.Interop;
namespace MochaTool.InteropGen;

public static class Parser
{
	/// <summary>
	/// Cached launch arguments so that we don't have to regenerate them every time
	/// </summary>
	private static string[] LaunchArgs = GetLaunchArgs();
	private static string[] GetLaunchArgs()
	{
		// Generate includes from vcxproj
		var includeDirs = VcxprojParser.ParseIncludes( "../Host/Host.vcxproj" );

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
		using var unit = CXTranslationUnit.Parse( index, path, LaunchArgs, ReadOnlySpan<CXUnsavedFile>.Empty, CXTranslationUnit_Flags.CXTranslationUnit_None );

		for ( int i = 0; i < unit.NumDiagnostics; ++i )
		{
			var diagnostics = unit.GetDiagnostic( (uint)i );
			Console.WriteLine( $"{diagnostics.Format( CXDiagnostic.DefaultDisplayOptions )}" );
		}

		var cursor = unit.Cursor;

		CXCursorVisitor cursorVisitor = ( CXCursor cursor, CXCursor parent, void* data ) =>
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
					units.Add( new Class( cursor.Spelling.ToString() ) );
					break;
				case CXCursorKind.CXCursor_StructDecl:
					units.Add( new Structure( cursor.Spelling.ToString() ) );
					break;
				case CXCursorKind.CXCursor_Namespace:
					units.Add( new Class( cursor.Spelling.ToString() )
					{
						IsNamespace = true
					} );
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

						var oName = cursor.LexicalParent.Spelling.ToString();
						var o = units.FirstOrDefault( x => x.Name == oName );
						var m = new Method( cursor.Spelling.ToString(), cursor.ReturnType.Spelling.ToString() )
						{
							IsStatic = cursor.IsStatic
						};

						if ( o == null )
						{
							Console.WriteLine( "No unit" );
							break;
						}

						CXCursorVisitor methodChildVisitor = ( CXCursor cursor, CXCursor parent, void* data ) =>
						{
							if ( cursor.Kind == CXCursorKind.CXCursor_ParmDecl )
							{
								var type = cursor.Type.ToString();
								var name = cursor.Spelling.ToString();

								var parameter = new Variable( name, type );

								m.Parameters.Add( parameter );
							}

							return CXChildVisitResult.CXChildVisit_Recurse;
						};

						cursor.VisitChildren( methodChildVisitor, default );

						if ( cursor.Kind == CXCursorKind.CXCursor_Constructor )
						{
							// Constructor specific stuff here
							m.ReturnType = $"{o.Name}*";
							m.Name = "Ctor";
							m.IsConstructor = true;
						}

						if ( cursor.CXXAccessSpecifier == CX_CXXAccessSpecifier.CX_CXXPublic || cursor.Kind == CXCursorKind.CXCursor_FunctionDecl )
							o.Methods.Add( m );

						break;
					}

				//
				// Field
				//
				case CXCursorKind.CXCursor_FieldDecl:
					{
						if ( !HasGenerateBindingsAttribute() )
							return CXChildVisitResult.CXChildVisit_Continue;

						var oName = cursor.LexicalParent.Spelling.ToString();
						var s = units.FirstOrDefault( x => x.Name == oName );

						if ( s == null )
							break;

						s.Fields.Add( new Variable( cursor.Spelling.ToString(), cursor.Type.ToString() ) );
						break;
					}

				default:
					break;
			}

			return CXChildVisitResult.CXChildVisit_Recurse;
		};

		cursor.VisitChildren( cursorVisitor, default );

		//
		// Remove all items with duplicate names
		//
		for ( int i = 0; i < units.Count; i++ )
		{
			var o = units[i];
			o.Methods = o.Methods.GroupBy( x => x.Name ).Select( x => x.First() ).ToList();
			o.Fields = o.Fields.GroupBy( x => x.Name ).Select( x => x.First() ).ToList();
		}

		//
		// Remove any units that have no methods or fields
		//
		units = units.Where( x => x.Methods.Count > 0 || x.Fields.Count > 0 ).ToList();

		//
		// Post-processing
		//
		foreach ( var o in units )
		{
			// Create a default constructor if one wasn't already defined
			if ( !o.Methods.Any( x => x.IsConstructor ) && o is not Class { IsNamespace: true } )
			{
				o.Methods.Add( new Method( "Ctor", $"{o.Name}*" )
				{
					IsConstructor = true
				} );
			}
		}

		return units;
	}
}
