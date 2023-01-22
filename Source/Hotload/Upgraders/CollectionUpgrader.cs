using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace Mocha.Hotload;

public class CollectionUpgrader : IMemberUpgrader
{
	/// <inheritdoc />
	public bool CanUpgrade( MemberInfo memberInfo )
	{
		return memberInfo.IsCollection();
	}

	/// <inheritdoc />
	public void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember )
	{
		object? oldValue = oldMember.GetValue( oldInstance );

		if ( oldValue == null )
			return;

		// For collections, we want to create a new instance of the collection and add the upgraded items to it
		object newValue = Activator.CreateInstance( newMember.Type )!;

		foreach ( object? item in (IEnumerable)oldValue! )
		{
			// We should really just be able to copy the collection across directly.
			object newItem = FormatterServices.GetUninitializedObject( item.GetType() );

			Upgrader.UpgradeInstance( item!, newItem! );

			// If this is a dictionary then we need to unwrap the key value pairs
			// because C# uses Add( Key, Value ) rather than Add( Pair ).

			if ( newMember.Type.GetInterface( nameof( IDictionary ) ) != null )
			{
				// Unwrap key value pair
				var pair = (dynamic)item;

				// Call Add
				newValue.GetType().GetMethod( "Add" )!.Invoke( newValue, new[] { pair.Key, pair.Value } );
			}
			else
			{
				newValue.GetType().GetMethod( "Add" )!.Invoke( newValue, new[] { item } );
			}
		}

		newMember.SetValue( newInstance, newValue );
	}
}
