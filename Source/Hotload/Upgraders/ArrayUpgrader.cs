using System.Reflection;

namespace Mocha.Hotload;

public class ArrayUpgrader : IMemberUpgrader
{
	public bool CanUpgrade( MemberInfo memberInfo )
	{
		if ( memberInfo is PropertyInfo propertyInfo )
		{
			return propertyInfo.PropertyType.IsArray;
		}
		else if ( memberInfo is FieldInfo fieldInfo )
		{
			return fieldInfo.FieldType.IsArray;
		}

		return false;
	}

	public void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember )
	{
		// TODO
	}
}
