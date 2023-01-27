using System.Reflection;
using System.Runtime.Serialization;

namespace Mocha.Hotload;

public class ClassUpgrader : IMemberUpgrader
{
	/// <summary>
	/// Dictionary of old hash codes and upgraded objects used
	/// for reference types
	/// </summary>
	public static Dictionary<int, object> UpgradedReferences { get; } = new();

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
				Log.Trace( $"'{oldMember.Name}' --> '{newMember.Name}': Type '{type.FullName}' upgrading to derived '{derivedType}' since it matches better" );

				type = derivedType;
			}
		}

		// Have we already upgraded this? If so, use a reference so that we're not 
		// duplicating the object.
		if ( UpgradedReferences.TryGetValue( oldValue.GetHashCode(), out var upgradedValue ) )
		{
			newMember.SetValue( newInstance, upgradedValue );

			Log.Trace( $"'{oldMember.Name}' --> '{newMember.Name}': Reference type '{type.FullName}' already upgraded, using reference" );
			return;
		}

		// Create a new instance of the class WITHOUT calling the constructor
		object newValue = FormatterServices.GetUninitializedObject( type );

		Log.Trace( $"Upgrading class '{oldMember.Type.Name}' (assembly hash {oldMember.Type.Assembly.GetHashCode()}) to '{newMember.Type.Name}' (assembly hash {newMember.Type.Assembly.GetHashCode()})" );

		// Save the reference for later
		UpgradedReferences[oldValue.GetHashCode()] = newValue;

		Upgrader.UpgradeInstance( oldValue!, newValue! );

		newMember.SetValue( newInstance, newValue );
	}
}
