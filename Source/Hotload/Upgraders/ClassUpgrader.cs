using System.Reflection;
using System.Runtime.Serialization;

namespace Mocha.Hotload;

public class ClassUpgrader : IMemberUpgrader
{
	/// <inheritdoc />
	public bool CanUpgrade( MemberInfo memberInfo )
	{
		return memberInfo.IsClass() || memberInfo.IsInterface();
	}

	/// <inheritdoc />
	public void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember )
	{
		object? oldValue = oldMember.GetValue( oldInstance );

		if ( oldValue == null )
			return;

		Type type = newMember.Type;

		// Check for a derived type if any
		{
			var oldDerivedType = oldValue.GetType();
			var derivedType = newInstance.GetType().Assembly.GetTypes().FirstOrDefault( x => x.FullName == oldDerivedType.FullName );

			if ( derivedType != null && type != derivedType )
			{
				type = derivedType;
			}
		}

		// Have we already upgraded this? If so, use a reference so that we're not 
		// duplicating the object.
		if ( Upgrader.UpgradedReferences.TryGetValue( oldValue.GetHashCode(), out var upgradedValue ) )
		{
			newMember.SetValue( newInstance, upgradedValue );
			return;
		}

		// Create a new instance of the class WITHOUT calling the constructor
		object newValue = FormatterServices.GetUninitializedObject( type );

		// Save the reference for later
		Upgrader.UpgradedReferences[oldValue.GetHashCode()] = newValue;
		Upgrader.UpgradeInstance( oldValue!, newValue! );

		newMember.SetValue( newInstance, newValue );
	}
}
