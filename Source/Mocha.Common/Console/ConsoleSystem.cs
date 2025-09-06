using System;
using System.Reflection;
using Mocha.Common.Console;

namespace Mocha.Common;

public static partial class ConsoleSystem
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
		Glue.ConsoleSystem.SetString( name, value );
	}

	public static void Set( string name, float value )
	{
		Glue.ConsoleSystem.SetFloat( name, value );
	}

	public static void Set( string name, bool value )
	{
		Glue.ConsoleSystem.SetBool( name, value );
	}

	public static void SetFromString( string name, string value )
	{
		Glue.ConsoleSystem.FromString( name, value );
	}
}
