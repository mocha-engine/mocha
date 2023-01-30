using System.Reflection;

namespace Mocha.Hotload;

/// <summary>
/// A member upgrader for strings.
/// </summary>
internal sealed class StringUpgrader : IMemberUpgrader
{
	/// <inheritdoc />
	public int Priority => 40;

	/// <inheritdoc />
	public bool CanUpgrade( MemberInfo memberInfo ) => memberInfo switch
	{
		PropertyInfo propertyInfo => propertyInfo.PropertyType == typeof( string ),
		FieldInfo fieldInfo => fieldInfo.FieldType == typeof( string ),
		_ => false
	};

	/// <inheritdoc />
	public void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember )
	{
		var oldValue = oldMember.GetValue( oldInstance );
		if ( oldValue is null )
			return;

		newMember.SetValue( newInstance, oldValue );
	}
}
