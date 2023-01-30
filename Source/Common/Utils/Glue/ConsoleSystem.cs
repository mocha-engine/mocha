namespace Mocha.Common;

public static class ConsoleSystem
{
	public static void Run( string command )
	{
		Glue.ConsoleSystem.Run( command );
	}

	public static float GetFloat( string name )
	{
		return Glue.ConsoleSystem.GetFloat( name );
	}

	public static bool GetBool( string name )
	{
		return Glue.ConsoleSystem.GetBool( name );
	}

	public static void Set( string name, string value )
	{
		// I figure we won't even want to think about what the underlying type is,
		// so let's use FromString by default
		Glue.ConsoleSystem.FromString( name, value );
	}

	public static void Set( string name, float value )
	{
		Glue.ConsoleSystem.SetFloat( name, value );
	}

	public static void Set( string name, bool value )
	{
		Glue.ConsoleSystem.SetBool( name, value );
	}
}
