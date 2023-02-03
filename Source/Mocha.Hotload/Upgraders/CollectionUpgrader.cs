using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace Mocha.Hotload;

/// <summary>
/// A member upgrader for any collections.
/// </summary>
internal sealed class CollectionUpgrader : IMemberUpgrader
{
	/// <inheritdoc />
	public int Priority => 30;

	/// <inheritdoc />
	public bool CanUpgrade( MemberInfo memberInfo )
	{
		return memberInfo.IsTypeCollection();
	}

	/// <inheritdoc />
	public void UpgradeMember( object oldInstance, UpgradableMember oldMember, object newInstance, UpgradableMember newMember )
	{
		var oldValue = oldMember.GetValue( oldInstance );
		if ( oldValue is null )
			return;

		// For collections, we want to create a new instance of the collection and add the upgraded items to it
		var newValue = Activator.CreateInstance( newMember.Type )!;

		foreach ( var item in (IEnumerable)oldValue! )
		{
			// We should really just be able to copy the collection across directly.
			var newItem = FormatterServices.GetUninitializedObject( item.GetType() );

			Upgrader.UpgradeInstance( item!, newItem! );

			// If this is a dictionary then we need to unwrap the key value pairs
			// because C# uses Add( Key, Value ) rather than Add( Pair ).

			if ( newMember.Type.GetInterface( nameof( IDictionary ) ) is not null )
			{
				// Unwrap key value pair
				var pair = (KeyValuePair<object, object>)item;

				// Call Add
				newValue.GetType().GetMethod( "Add" )!.Invoke( newValue, new[] { pair.Key, pair.Value } );
			}
			else
				newValue.GetType().GetMethod( "Add" )!.Invoke( newValue, new[] { item } );
		}

		newMember.SetValue( newInstance, newValue );
	}
}
