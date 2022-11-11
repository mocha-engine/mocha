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
}
