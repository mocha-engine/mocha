using System.Reflection;

namespace Mocha.Hotload;

public class PrimitiveUpgrader : IMemberUpgrader
{
	public bool CanUpgrade( MemberInfo memberInfo )
	{
		if ( memberInfo is PropertyInfo propertyInfo )
		{
			return propertyInfo.PropertyType.IsPrimitive;
		}
		else if ( memberInfo is FieldInfo fieldInfo )
		{
			return fieldInfo.FieldType.IsPrimitive;
		}

		return false;
	}

	public void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember )
	{
		object? oldValue = oldMember.GetValue( oldInstance );

		if ( oldValue == null )
			return;

		newMember.SetValue( newInstance, oldValue );
	}
}
