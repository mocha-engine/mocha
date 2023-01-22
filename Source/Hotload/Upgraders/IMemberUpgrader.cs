using System.Reflection;

namespace Mocha.Hotload;

public interface IMemberUpgrader
{
	bool CanUpgrade( MemberInfo memberInfo );
	void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember );
}
