using System.CodeDom.Compiler;

sealed class NativeCodeGenerator : BaseCodeGenerator
{
	public NativeCodeGenerator( List<IUnit> units ) : base( units )
	{
	}

	public string GenerateNativeCode( string headerPath )
	{
		var (baseTextWriter, writer) = Utils.CreateWriter();

		writer.WriteLine( GetHeader() );
		writer.WriteLine();

		writer.WriteLine( "#pragma once" );
		writer.WriteLine( $"#include \"..\\{headerPath}\"" );

		writer.WriteLine();

		foreach ( var unit in Units )
		{
			if ( unit is Class c )
			{
				GenerateClassCode( ref writer, c );
			}

			if ( unit is Structure s )
			{
				GenerateStructCode( ref writer, s );
			}

			writer.WriteLine();
		}

		return baseTextWriter.ToString();
	}

	private void GenerateStructCode( ref IndentedTextWriter writer, Structure s )
	{

	}

	private void GenerateClassCode( ref IndentedTextWriter writer, Class c )
	{
		foreach ( var method in c.Methods )
		{
			var args = method.Parameters;

			if ( !method.IsStatic )
				args = args.Prepend( new Variable( "instance", $"{c.Name}*" ) ).ToList();

			var argStr = string.Join( ", ", args.Select( x =>
			{
				if ( x.Type == "string" )
				{
					return $"const char* {x.Name}";
				}

				return $"{x.Type} {x.Name}";
			} ) );

			var signature = $"extern \"C\" inline {method.ReturnType} __{c.Name}_{method.Name}( {argStr} )";
			var body = "";
			var @params = string.Join( ", ", method.Parameters.Select( x => x.Name ) );

			if ( method.IsConstructor )
			{
				body += $"return new {c.Name}( {@params} );";
			}
			else if ( method.IsDestructor )
			{
				body += $"instance->~{c.Name}( {@params} );";
			}
			else
			{
				var accessor = method.IsStatic ? $"{c.Name}::" : "instance->";

				if ( method.ReturnType == "void" )
					body += $"{accessor}{method.Name}( {@params} );";
				else if ( method.ReturnType == "std::string" )
					body += $"std::string text = {accessor}{method.Name}( {@params} );\r\nconst char* cstr = text.c_str();\r\nchar* dup = _strdup(cstr);\r\nreturn dup;";
				else
					body += $"return {accessor}{method.Name}( {@params} );";
			}

			writer.WriteLine( signature );
			writer.WriteLine( "{" );
			writer.Indent++;

			writer.WriteLine( body );

			writer.Indent--;
			writer.WriteLine( "}" );
		}
	}
}
