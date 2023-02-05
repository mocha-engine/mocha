namespace Mocha.Common;

[AttributeUsage( AttributeTargets.Method, Inherited = false, AllowMultiple = true )]
public sealed class ServerOnlyAttribute : Attribute
{
	public ServerOnlyAttribute()
	{
	}
}
