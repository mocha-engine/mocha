using System.Reflection;

namespace Mocha.Hotload.Upgrading;

/// <summary>
/// A contract defining an object that can upgrade members from two different assemblies.
/// </summary>
internal interface IMemberUpgrader
{
	/// <summary>
	/// The priority level at which to place the upgrader. Higher means more priority.
	/// </summary>
	int Priority { get; }

	/// <summary>
	/// Returns whether or not the upgrader supports upgrading the member.
	/// </summary>
	/// <returns>Whether or not the upgrader supports upgrading the member.</returns>
	bool CanUpgrade( MemberInfo memberInfo );

	/// <summary>
	/// Performs the upgrade for this member. Copies the contents of <see ref="oldInstance"/> into <see ref="newInstance"/>.
	/// </summary>
	void UpgradeMember( object? oldInstance, UpgradableMember oldMember, object? newInstance, UpgradableMember newMember );
}
