namespace Mocha.Common;

public static class ConCmd
{
	public abstract class BaseAttribute : Attribute
	{
		public string Name { get; init; }
		public CVarFlags Flags { get; init; }
		public string Description { get; init; }

		public BaseAttribute( string name, CVarFlags flags, string description )
		{
			Name = name;
			Flags = flags;
			Description = description;
		}
	}

	// public class ServerAttribute
	// public class ClientAttribute

	public sealed class TestAttribute : BaseAttribute
	{
		public TestAttribute( string name )
			: base( name, CVarFlags.None, "" )
		{

		}

		public TestAttribute( string name, string description )
			: base( name, CVarFlags.None, description )
		{

		}
	}
}
