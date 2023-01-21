using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Mocha.Hotload;

/// <summary>
/// Move fields from the previously loaded assembly
/// to the newly loaded assembly
/// </summary>
public class FieldUpgrader
{
	private readonly Assembly baseAssembly;
	private readonly Assembly newAssembly;

	public FieldUpgrader( Assembly baseAssembly, Assembly newAssembly )
	{
		this.baseAssembly = baseAssembly;
		this.newAssembly = newAssembly;
	}

	public void Upgrade( object oldInstance, object newInstance )
	{
		UpgradeFields( oldInstance, newInstance );
	}

	/// <summary>
	/// Recursive function that will copy the values from the old instance
	/// to a new one.
	/// </summary>
	private void UpgradeFields( object oldInstance, object newInstance )
	{
		if ( oldInstance == null )
			return;

		if ( newInstance == null )
			return;

		// TODO: Structs

		// Get all fields from the old instance
		var fields = oldInstance.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

		// For each field:
		// - If it's a reference type, we will want to upgrade it if it's a class
		//   and not a delegate
		// - Otherwise, copy the value
		foreach ( var field in fields )
		{
			// Check if field is a backing field and skip
			if ( field.GetCustomAttribute<CompilerGeneratedAttribute>() != null )
				continue;

			// Get the old value - this is what we'll copy over
			var oldValue = field.GetValue( oldInstance )!;

			// Find new field - this is the one with a matching name
			var newField = newInstance.GetType().GetField( field.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

			// Has the field been deleted?
			if ( newField == null )
				continue;

			// For things like strings, integers, etc. we can just use SetValue
			if ( field.FieldType.IsPrimitive || field.FieldType == typeof( string ) )
			{
				// Copy the value directly
				newField.SetValue( newInstance, oldValue );

				Log.Trace( $"Copied value '{oldValue}' to field {field.Name} of type {field.FieldType}" );
			}
			else if ( field.FieldType.IsClass && !field.FieldType.IsSubclassOf( typeof( Delegate ) ) )
			{
				Log.Trace( $"Upgrading class in field {field.Name}" );

				// Create instance of new class - don't call the constructor
				var newValue = FormatterServices.GetUninitializedObject( newField.FieldType );

				// Reference type, upgrade it
				UpgradeFields( oldValue, newValue );
				newField.SetValue( newInstance, newValue );
			}
			else
			{
				// This is something we don't know how to upgrade, so skip for now
				Log.Trace( $"Don't know how to upgrade field {field.Name} of type {field.FieldType}" );
			}
		}
	}
}
