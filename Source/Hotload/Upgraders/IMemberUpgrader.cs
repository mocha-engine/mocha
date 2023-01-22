using System.Reflection;

namespace Mocha.Hotload;

public interface IMemberUpgrader
{
	/// <summary>
	/// Does this upgrader support upgrading for this member?
	/// </summary>
	bool CanUpgrade( MemberInfo memberInfo );

	/// <summary>
	/// Performs the upgrade for this member: copies the contents of
	/// oldInstance into newInstance.
	/// </summary>
	void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember );
}
