namespace Mocha.Common;

public static class ConCmd
{
	public abstract class BaseAttribute : Attribute
	{
		internal string? Name { get; init; }
		internal CVarFlags Flags { get; init; }
		internal string? Description { get; init; }

		public BaseAttribute( string? name, CVarFlags flags, string? description )
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
		public TestAttribute()
			: base( null, CVarFlags.None, null )
		{

		}
		public TestAttribute( string name )
			: base( name, CVarFlags.None, null )
		{

		}

		public TestAttribute( string name, string description )
			: base( name, CVarFlags.None, description )
		{

		}
	}

	public sealed class CheatAttribute : BaseAttribute
	{
		public CheatAttribute()
			: base( null, CVarFlags.Cheat, null )
		{

		}
		public CheatAttribute( string name )
			: base( name, CVarFlags.Cheat, null )
		{

		}

		public CheatAttribute( string name, string description )
			: base( name, CVarFlags.Cheat, description )
		{

		}
	}
}
