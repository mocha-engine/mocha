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

		// Create a new instance of the class WITHOUT calling the constructor
		object newValue = FormatterServices.GetUninitializedObject( newMember.Type );

		Log.Trace( $"Upgrading class '{oldMember.Type.Name}' (assembly hash {oldMember.Type.Assembly.GetHashCode()}) to '{newMember.Type.Name}' (assembly hash {newMember.Type.Assembly.GetHashCode()})" );

		Upgrader.UpgradeInstance( oldValue!, newValue! );

		newMember.SetValue( newInstance, newValue );
	}
}
