using System.Reflection;

namespace Mocha.Hotload;

internal interface IMemberUpgrader
{
	/// <summary>
	/// The priority level at which to place the upgrader. Higher means more priority.
	/// </summary>
	int Priority { get; }

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
