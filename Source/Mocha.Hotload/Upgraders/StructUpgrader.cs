﻿using Mocha.Hotload.Util;
using System.Reflection;
using System.Runtime.Serialization;

namespace Mocha.Hotload.Upgrading.Upgraders;

/// <summary>
/// A member upgrader for structs.
/// </summary>
internal sealed class StructUpgrader : IMemberUpgrader
{
	/// <inheritdoc />
	public int Priority => 10;

	/// <inheritdoc />
	public bool CanUpgrade( MemberInfo memberInfo )
	{
		return memberInfo.IsTypeStruct();
	}

	/// <inheritdoc />
	public void UpgradeMember( object? oldInstance, UpgradableMember oldMember, object? newInstance, UpgradableMember newMember )
	{
		var oldValue = oldMember.GetValue( oldInstance );
		if ( oldValue is null )
			return;

		// Create a new instance of the struct WITHOUT calling the constructor
		var newValue = FormatterServices.GetUninitializedObject( newMember.Type );

		Upgrader.UpgradeInstance( oldValue!, newValue! );

		newMember.SetValue( newInstance, newValue );
	}
}
