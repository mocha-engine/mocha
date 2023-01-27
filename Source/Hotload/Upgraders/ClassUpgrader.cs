using System.Reflection;
using System.Runtime.Serialization;

namespace Mocha.Hotload;

public class ClassUpgrader : IMemberUpgrader
{
	/// <inheritdoc />
	public bool CanUpgrade( MemberInfo memberInfo )
	{
		return memberInfo.IsClass();
	}

	/// <inheritdoc />
	public void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember )
	{
		object? oldValue = oldMember.GetValue( oldInstance );

		if ( oldValue == null )
			return;

		Type type = newMember.Type;

		if ( type.IsAbstract )
		{
			var oldDerivedType = oldValue.GetType();

			type = newMember.Type.Assembly.GetTypes().First( x => x.FullName == oldDerivedType.FullName );
		}

		// Create a new instance of the class WITHOUT calling the constructor
		object newValue = FormatterServices.GetUninitializedObject( type );

		Log.Trace( $"Upgrading class '{oldMember.Type.Name}' (assembly hash {oldMember.Type.Assembly.GetHashCode()}) to '{newMember.Type.Name}' (assembly hash {newMember.Type.Assembly.GetHashCode()})" );

		Upgrader.UpgradeInstance( oldValue!, newValue! );

		newMember.SetValue( newInstance, newValue );
	}
}
