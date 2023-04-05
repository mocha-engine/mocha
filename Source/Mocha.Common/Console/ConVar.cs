namespace Mocha.Common;

public static class ConVar
{
	public abstract class BaseAttribute : Attribute
	{
		public required string Name { get; init; }
		public required CVarFlags Flags { get; init; }
	public required string Description { get; init; }
}

public class TestAttribute : Attribute
{

}
}
