using System.Reflection;

namespace Mocha.Hotload.Upgrading.Upgraders;

/// <summary>
/// A member upgrader for primitives.
/// </summary>
internal sealed class PrimitiveUpgrader : IMemberUpgrader
{
	/// <inheritdoc />
	public int Priority => 50;

	/// <inheritdoc />
	public bool CanUpgrade( MemberInfo memberInfo ) => memberInfo switch
	{
		PropertyInfo propertyInfo => propertyInfo.PropertyType.IsPrimitive,
		FieldInfo fieldInfo => fieldInfo.FieldType.IsPrimitive,
		_ => false
	};

	/// <inheritdoc />
	public void UpgradeMember( object? oldInstance, UpgradableMember oldMember, object? newInstance, UpgradableMember newMember )
	{
		var oldValue = oldMember.GetValue( oldInstance );
		if ( oldValue is null )
			return;

		newMember.SetValue( newInstance, oldValue );
	}
}
