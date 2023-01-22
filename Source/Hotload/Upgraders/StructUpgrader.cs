using System.Reflection;
using System.Runtime.Serialization;

namespace Mocha.Hotload;

public class StructUpgrader : IMemberUpgrader
{
	/// <inheritdoc />
	public bool CanUpgrade( MemberInfo memberInfo )
	{
		return memberInfo.IsStruct();
	}

	/// <inheritdoc />
	public void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember )
	{
		object? oldValue = oldMember.GetValue( oldInstance );

		if ( oldValue == null )
			return;

		// Create a new instance of the struct WITHOUT calling the constructor
		object newValue = FormatterServices.GetUninitializedObject( newMember.Type );

		Upgrader.UpgradeInstance( oldValue!, newValue! );

		newMember.SetValue( newInstance, newValue );
	}
}
