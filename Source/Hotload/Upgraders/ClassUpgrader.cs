using System.Reflection;
using System.Runtime.Serialization;

namespace Mocha.Hotload;

public class ClassUpgrader : IMemberUpgrader
{
	public bool CanUpgrade( MemberInfo memberInfo )
	{
		return memberInfo.IsClass();
	}

	public void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember )
	{
		object? oldValue = oldMember.GetValue( oldInstance );

		// Create a new instance of the class WITHOUT calling the constructor
		object newValue = FormatterServices.GetUninitializedObject( newMember.Type );

		Upgrader.UpgradeInstance( oldValue!, newValue! );

		newMember.SetValue( newInstance, newValue );
	}
}
