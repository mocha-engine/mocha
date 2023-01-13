using System.CodeDom.Compiler;

static class Utils
{
	public static string GetManagedType( string nativeType )
	{
		// Trim whitespace from beginning / end (if it exists)
		nativeType = nativeType.Trim();

		// Remove the "const" keyword
		if ( nativeType.StartsWith( "const" ) )
			nativeType = nativeType[5..].Trim();

		// Create a dictionary to hold the mapping between native and managed types
		var lookupTable = new Dictionary<string, string>()
		{
			// Native type		Managed type
			//-------------------------------
			{ "void",           "void" },
			{ "uint32_t",       "uint" },
			{ "size_t",         "uint" },

			{ "char**",         "ref string" },
			{ "char **",        "ref string" },
			{ "char*",          "string" },
			{ "char *",         "string" },
			{ "void*",          "IntPtr" },
			{ "void *",         "IntPtr" },

			// STL
			{ "std::string",    "/* UNSUPPORTED */ string" },

			// GLM
			{ "glm::vec2",      "Vector2" },
			{ "glm::vec3",      "Vector3" },
			{ "glm::mat4",      "Matrix4x4" },
			{ "glm::quat",      "Rotation" },

			// Custom
			{ "Quaternion",     "Rotation" },
			{ "InteropStruct",  "IInteropArray" },
		};

		// Check if the native type is a reference
		if ( nativeType.EndsWith( "&" ) )
			return GetManagedType( nativeType[0..^1] );

		// Check if the native type is in the lookup table
		if ( lookupTable.ContainsKey( nativeType ) )
		{
			// Bonus: Emit a compiler warning if the native type is std::string
			if ( nativeType == "std::string" )
			{
				// There's a better API that does this but I can't remember what it is
				// TODO: Show position of the warning (line number, file name)
				Console.WriteLine( "warning IG0001: std::string is not supported in managed code. Use a C string instead." );
			}

			return lookupTable[nativeType];
		}

		// Check if the native type is a pointer
		if ( nativeType.EndsWith( "*" ) )
			return GetManagedType( nativeType[..^1].Trim() ); // We'll return the basic type, because we handle pointers on the C# side now

		// Return the native type if it is not in the lookup table
		return nativeType;
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
