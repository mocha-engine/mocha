namespace Mocha.InteropGen;

public struct VariableType
{
	public string Name { get; set; }
	public string NativeType { get; set; }
	public string CSharpMarshalAs
	{
		get
		{
			if ( NativeType == "const char*" || NativeType == "string_t" || NativeType == "std::string" )
			{
				return "[MarshalAs( UnmanagedType.LPStr )]";
			}
			else
			{
				return "";
			}
		}
	}

	public string CSharpType
	{
		get
		{
			if ( NativeType == "const char*" || NativeType == "string_t" || NativeType == "std::string" )
			{
				return "string";
			}
			else if ( NativeType == "bool" )
			{
				return "bool";
			}
			else if ( NativeType == "int" )
			{
				return "int";
			}
			else if ( NativeType.EndsWith( "*" ) )
			{
				return "IntPtr";
			}
			else if ( NativeType.StartsWith( "std::function" ) )
			{
				return "IntPtr";
			}
			else
			{
				return "IntPtr";
			}
		}
	}

	public VariableType( string nativeType, string name )
	{
		Name = name;
		NativeType = nativeType;
	}
}
