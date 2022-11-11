using ClangSharp.Interop;

public static class Program
{
	private static bool Debug => false;

	public static void Main()
	{
		List<IUnit> objects = new();

		unsafe
		{
			var file = "InteropGen2/test.h";

			Console.WriteLine( $"Scanning {file}" );

			using var index = CXIndex.Create();
			using var unit = CXTranslationUnit.Parse( index, file, new string[] { "-x", "c++" }, ReadOnlySpan<CXUnsavedFile>.Empty, CXTranslationUnit_Flags.CXTranslationUnit_None );

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

				switch ( cursor.Kind )
				{
					case CXCursorKind.CXCursor_ClassDecl:
						objects.Add( new Class( cursor.Spelling.ToString() ) );
						break;
					case CXCursorKind.CXCursor_StructDecl:
						objects.Add( new Structure( cursor.Spelling.ToString() ) );
						break;
					case CXCursorKind.CXCursor_Constructor:
					case CXCursorKind.CXCursor_CXXMethod:
						{
							var oName = cursor.LexicalParent.Spelling.ToString();
							var o = objects.First( x => x.Name == oName );
							var m = new Method( cursor.Spelling.ToString(), cursor.ReturnType.Spelling.ToString() );

							CXCursorVisitor methodChildVisitor = ( CXCursor cursor, CXCursor parent, void* data ) =>
							{
								if ( cursor.Kind == CXCursorKind.CXCursor_ParmDecl )
								{
									m.Parameters.Add( Utils.CppTypeToCsharp( cursor.Type.ToString() ) + " " + cursor.Spelling.ToString() );
								}

								return CXChildVisitResult.CXChildVisit_Recurse;
							};

							cursor.VisitChildren( methodChildVisitor, default );

							if ( cursor.CXXAccessSpecifier == CX_CXXAccessSpecifier.CX_CXXPublic )
								o.Methods.Add( m );

							break;
						}
					case CXCursorKind.CXCursor_FieldDecl:
						{
							var oName = cursor.LexicalParent.Spelling.ToString();
							var s = objects.First( x => x.Name == oName );
							s.Fields.Add( new Field( cursor.Spelling.ToString(), cursor.Type.ToString() ) );
							break;
						}
				}

				if ( Debug )
					Console.WriteLine( $"{cursor.Kind,32} {cursor.Spelling,24} {cursor.Location,48} {cursor.LexicalParent.Kind,32}" );

				return CXChildVisitResult.CXChildVisit_Recurse;
			};

			cursor.VisitChildren( cursorVisitor, default );

			//
			// Classes
			//
			Console.WriteLine();
			Console.WriteLine( "Objects:" );
			foreach ( var o in objects )
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
			foreach ( var o in objects )
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
	}
}
