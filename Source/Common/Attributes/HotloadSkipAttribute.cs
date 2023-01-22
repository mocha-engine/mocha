namespace Mocha.Common;

[System.AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true )]
public sealed class HotloadSkipAttribute : Attribute
{
	public HotloadSkipAttribute()
	{
	}
}
