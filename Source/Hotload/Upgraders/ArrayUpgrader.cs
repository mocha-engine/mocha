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
		object? oldValue = oldMember.GetValue( oldInstance );

		if ( oldValue == null )
			return;

		var oldArray = (Array)oldValue;
		var newArray = Array.CreateInstance( newMember.Type.GetElementType()!, oldArray.Length );

		for ( int i = 0; i < oldArray.Length; i++ )
		{
			newArray.SetValue( oldArray.GetValue( i ), i );
		}

		newMember.SetValue( newInstance, newArray );
	}
}
