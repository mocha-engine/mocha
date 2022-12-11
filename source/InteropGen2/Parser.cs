using ClangSharp.Interop;

public static class Parser
{
	private static bool Debug => false;

	private static string[] GetLaunchArgs()
	{
		var includeDirs = new string[]
		{
			"C:\\VulkanSDK\\1.3.224.1\\Include",
			"C:\\Users\\Alex\\vcpkg\\installed\\x64-windows\\include",
			"C:\\Users\\Alex\\vcpkg\\installed\\x64-windows\\include\\SDL2",
			"../Host/",
			"../Host/ThirdParty/Renderdoc",
			"../Host/ThirdParty/vk-bootstrap/src",
			"../Host/ThirdParty/imgui",
			"../Host/ThirdParty/JoltPhysics",
		};

		var args = new List<string>();
		args.Add( "-x" );
		args.Add( "c++" );
		args.Add( "-fparse-all-comments" );

		args.AddRange( includeDirs.Select( x => "-I" + x ) );

		return args.ToArray();
	}

	public unsafe static List<IUnit> GetUnits( string path )
	{
		List<IUnit> units = new();

		using var index = CXIndex.Create();
		using var unit = CXTranslationUnit.Parse( index, path, GetLaunchArgs(), ReadOnlySpan<CXUnsavedFile>.Empty, CXTranslationUnit_Flags.CXTranslationUnit_None );

		for ( int i = 0; i < unit.NumDiagnostics; ++i )
		{
			var diagnostics = unit.GetDiagnostic( (uint)i );
			Console.WriteLine( $"{diagnostics.Format( CXDiagnostic.DefaultDisplayOptions )}" );
		}

		var cursor = unit.Cursor;

		//
		// Display all tokens
		//

		if ( Debug )
		{
			Console.WriteLine();
			Console.WriteLine( $"{"Kind",32} {"Spelling",24} {"Location",48} {"Lexical Parent",32}" );
			Console.WriteLine( new string( '-', 32 + 24 + 48 + 32 + 3 ) );
		}

		CXCursorVisitor cursorVisitor = ( CXCursor cursor, CXCursor parent, void* data ) =>
		{
			if ( !cursor.Location.IsFromMainFile )
				return CXChildVisitResult.CXChildVisit_Continue;

			if ( cursor.RawCommentText.ToString() == "//@InteropGen ignore" )
				return CXChildVisitResult.CXChildVisit_Continue;

			switch ( cursor.Kind )
			{
				case CXCursorKind.CXCursor_ClassDecl:
					if ( cursor.RawCommentText.ToString() == "//@InteropGen generate class" )
						units.Add( new Class( cursor.Spelling.ToString() ) );
					break;
				case CXCursorKind.CXCursor_StructDecl:
					if ( cursor.RawCommentText.ToString() == "//@InteropGen generate struct" )
						units.Add( new Structure( cursor.Spelling.ToString() ) );
					break;
				case CXCursorKind.CXCursor_Namespace:
					if ( cursor.RawCommentText.ToString() == "//@InteropGen generate class" )
						units.Add( new Class( cursor.Spelling.ToString() )
						{
							IsNamespace = true
						} );
					break;
				case CXCursorKind.CXCursor_Constructor:
				case CXCursorKind.CXCursor_CXXMethod:
				case CXCursorKind.CXCursor_FunctionDecl:
					{
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
				case CXCursorKind.CXCursor_FieldDecl:
					{
						var oName = cursor.LexicalParent.Spelling.ToString();
						var s = units.FirstOrDefault( x => x.Name == oName );

						if ( s == null )
							break;

						s.Fields.Add( new Variable( cursor.Spelling.ToString(), cursor.Type.ToString() ) );
						break;
					}
			}

			if ( Debug )
			{
				Console.WriteLine( $"{cursor.Kind,32} {cursor.Spelling,24} {cursor.Type,48} {cursor.LexicalParent.Kind,32}" );
			}

			return CXChildVisitResult.CXChildVisit_Recurse;
		};

		cursor.VisitChildren( cursorVisitor, default );

		if ( Debug )
		{
			//
			// Classes
			//
			Console.WriteLine();
			Console.WriteLine( "Objects:" );
			foreach ( var o in units )
			{
				if ( o is not Class c )
					continue;

				Console.WriteLine( $"Class {c}:" );

				foreach ( var m in o.Methods )
				{
					Console.WriteLine( $"\tMethod - {m}" );
				}

				foreach ( var f in o.Fields )
				{
					Console.WriteLine( $"\tField - {f}" );
				}
			}

			//
			// Structs
			//
			foreach ( var o in units )
			{
				if ( o is not Structure s )
					continue;

				Console.WriteLine( $"Struct {s}:" );

				foreach ( var m in o.Methods )
				{
					Console.WriteLine( $"\tMethod - {m}" );
				}

				foreach ( var f in o.Fields )
				{
					Console.WriteLine( $"\tField - {f}" );
				}
			}
		}

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

		//
		// Remove all items with duplicate names
		//
		for ( int i = 0; i < units.Count; i++ )
		{
			var o = units[i];
			o.Methods = o.Methods.GroupBy( x => x.Name ).Select( x => x.First() ).ToList();
			o.Fields = o.Fields.GroupBy( x => x.Name ).Select( x => x.First() ).ToList();
		}

		return units;
	}
}
