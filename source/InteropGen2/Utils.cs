using System.CodeDom.Compiler;

static class Utils
{
	public static string GetManagedType( string nativeType )
	{
		if ( nativeType == "void" )
			return nativeType;

		if ( nativeType == "glm::vec2" )
			return "Vector2";

		if ( nativeType == "glm::vec3" )
			return "Vector3";

		if ( nativeType == "glm::mat4" )
			return "Matrix4x4";

		if ( nativeType.EndsWith( "*" ) )
		{
			// Pointer
			return "IntPtr";
		}

		return nativeType switch
		{
			"std::string" => "[MarshalAs( UnmanagedType.LPStr )] string",

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
