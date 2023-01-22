using Mocha.Common;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mocha.Hotload;

public static class Upgrader
{
	private static List<IMemberUpgrader> Upgraders { get; set; }

	/// <summary>
	/// This must be called before invoking any other functions. Ideally, this should be
	/// invoked at the very start of the program.
	/// </summary>
	public static void Init()
	{
		// We could alternatively use static constructors
		// (https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-constructors)
		// but these are lazy loaded and we want to make sure all upgraders are set up
		// ahead-of-time rather than setting them up on-demand.

		// These actually have a specific hierarchy / order, so we don't use reflection here
		// at the moment
		Upgraders = new List<IMemberUpgrader>()
		{
			new ArrayUpgrader(),
			new PrimitiveUpgrader(),
			new StringUpgrader(),

			new ClassUpgrader()
		};
	}

	public static void UpgradeInstance( object? oldInstance, object? newInstance )
	{
		if ( oldInstance == null )
		{
			Log.Warning( "UpgradeInstance bailing because oldInstance was null!" );
			return;
		}

		if ( newInstance == null )
		{
			Log.Warning( "UpgradeInstance bailing because newInstance was null!" );
			return;
		}

		var oldType = oldInstance.GetType();
		var newType = newInstance.GetType();

		// Get all fields from the old instance
		var oldMembers = oldType.GetMembers( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

		// For each field:
		// - If it's a reference type, we will want to upgrade it if it's a class
		//   and not a delegate
		// - Otherwise, copy the value
		foreach ( var oldMember in oldMembers )
		{
			//
			// Old member
			//
			if ( oldMember.GetCustomAttribute<CompilerGeneratedAttribute>() != null )
				continue;

			if ( oldMember.GetCustomAttribute<HotloadSkipAttribute>() != null )
				continue;

			var oldUpgradable = UpgradableMember.FromMember( oldMember );

			// Can we upgrade this?
			if ( oldUpgradable == null )
				continue;

			//
			// New member
			//
			var newMember = newType.GetMember( oldMember.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
								   .FirstOrDefault();

			// Does this member exist? (eg. might have been deleted)
			if ( newMember == null )
				continue;

			if ( newMember.GetCustomAttribute<HotloadSkipAttribute>() != null )
				continue;

			var newUpgradable = UpgradableMember.FromMember( newMember );

			// Can we upgrade this?
			if ( newUpgradable == null )
				continue;

			//
			// Upgrade!
			//
			bool wasUpgraded = false;

			foreach ( var upgrader in Upgraders )
			{
				if ( !upgrader.CanUpgrade( oldMember ) )
					continue;

				upgrader.UpgradeMember( oldInstance, oldUpgradable, newInstance, newUpgradable );
				wasUpgraded = true;
				Log.Trace( $"Copied {newMember.MemberType.ToString().ToLower()} '{newMember.Name}' in '{newType.Name}'" );

				break;
			}

			if ( !wasUpgraded )
			{
				Log.Trace( $"Don't know how to upgrade {oldMember.MemberType.ToString().ToLower()} '{oldMember.Name}' in '{oldType.Name}'" );
			}
		}
	}
}
