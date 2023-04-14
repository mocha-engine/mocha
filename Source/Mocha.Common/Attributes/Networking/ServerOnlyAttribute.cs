namespace Mocha.Common;

[AttributeUsage( AttributeTargets.Class |
	AttributeTargets.Interface |
	AttributeTargets.Struct |
	AttributeTargets.Enum |
	AttributeTargets.Field |
	AttributeTargets.Property |
	AttributeTargets.Constructor |
	AttributeTargets.Method |
	AttributeTargets.Delegate |
	AttributeTargets.Event, Inherited = true, AllowMultiple = false )]
public sealed class ServerOnlyAttribute : Attribute
{
	public ServerOnlyAttribute()
	{
	}
}
