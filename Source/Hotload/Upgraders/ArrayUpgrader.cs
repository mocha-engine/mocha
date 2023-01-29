using System.Reflection;

namespace Mocha.Hotload;

/// <summary>
/// A member upgrader for arrays.
/// </summary>
internal class ArrayUpgrader : IMemberUpgrader
{
	/// <inheritdoc />
	public bool CanUpgrade( MemberInfo memberInfo ) => memberInfo switch
	{
		PropertyInfo propertyInfo => propertyInfo.PropertyType.IsArray,
		FieldInfo fieldInfo => fieldInfo.FieldType.IsArray,
		_ => false
	};

	/// <inheritdoc />
	public void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember )
	{
		var oldValue = oldMember.GetValue( oldInstance );
		if ( oldValue is null )
			return;

		var oldArray = (Array)oldValue;
		var newArray = Array.CreateInstance( newMember.Type.GetElementType()!, oldArray.Length );

		for ( int i = 0; i < oldArray.Length; i++ )
			newArray.SetValue( oldArray.GetValue( i ), i );

		newMember.SetValue( newInstance, newArray );
	}
}
