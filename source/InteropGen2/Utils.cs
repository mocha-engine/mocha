using System.CodeDom.Compiler;

static class Utils
{
	public static string GetManagedType( string nativeType )
	{
		if ( nativeType == "void" )
			return nativeType;

		if ( nativeType.EndsWith( "*" ) )
		{
			// Pointer
			return "IntPtr";
		}

		return nativeType switch
		{
			"std::string" => "string",

			_ => nativeType
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
