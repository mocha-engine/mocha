using System.CodeDom.Compiler;

static class Utils
{
	public static string CppTypeToCsharp( string type )
	{
		if ( type.EndsWith( "*" ) )
		{
			// Pointer
			return "IntPtr";
		}

		return type switch
		{
			"std::string" => "string",

			_ => type
		};
	}

	public static (StringWriter StringWriter, IndentedTextWriter TextWriter) CreateWriter()
	{
		var baseTextWriter = new StringWriter();

		var writer = new IndentedTextWriter( baseTextWriter, "    " )
		{
			Indent = 0
		};

		return (baseTextWriter, writer);
	}
}
