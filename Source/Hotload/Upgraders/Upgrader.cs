using Mocha.Common;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mocha.Hotload;

/// <summary>
/// The core class for upgrading members when swapping assemblies.
/// </summary>
internal static class Upgrader
{
	/// <summary>
	/// Dictionary of old hash codes and upgraded objects used
	/// for reference types
	/// </summary>
	internal static Dictionary<int, object> UpgradedReferences { get; } = new();

	private static List<IMemberUpgrader> s_upgraders { get; set; } = null!;

	/// <summary>
	/// This must be called before invoking any other functions. Ideally, this should be
	/// invoked at the very start of the program.
	/// </summary>
	internal static void Init()
	{
		// We could alternatively use static constructors
		// (https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-constructors)
		// but these are lazy loaded and we want to make sure all upgraders are set up
		// ahead-of-time rather than setting them up on-demand.

		var upgraderTypes = Assembly.GetExecutingAssembly().GetTypes()
			.Where( t => t.GetInterface( nameof( IMemberUpgrader ) ) is not null )
			.ToImmutableArray();

		var upgraders = new IMemberUpgrader[upgraderTypes.Length];
		for ( var i = 0; i < upgraders.Length; i++ )
			upgraders[i] = (IMemberUpgrader)Activator.CreateInstance( upgraderTypes[i] )!;
		s_upgraders = upgraders.OrderByDescending( upgrader => upgrader.Priority ).ToList();
	}

	internal static void UpgradeInstance( object? oldInstance, object? newInstance )
	{
		// Bail
		if ( oldInstance is null || newInstance is null )
			return;

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
			if ( oldMember.GetCustomAttribute<CompilerGeneratedAttribute>() is not null )
				continue;

			if ( oldMember.GetCustomAttribute<HotloadSkipAttribute>() is not null )
				continue;

			var oldUpgradable = UpgradableMember.FromMember( oldMember );

			// Can we upgrade this?
			if ( oldUpgradable is null )
				continue;

			//
			// New member
			//
			var newMember = newType.GetMember( oldMember.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
								   .FirstOrDefault();

			// Does this member exist? (eg. might have been deleted)
			if ( newMember is null )
				continue;

			if ( newMember.GetCustomAttribute<HotloadSkipAttribute>() != null )
				continue;

			var newUpgradable = UpgradableMember.FromMember( newMember );

			// Can we upgrade this?
			if ( newUpgradable is null )
				continue;

			//
			// Upgrade!
			//
			var wasUpgraded = false;

			foreach ( var upgrader in s_upgraders )
			{
				if ( !upgrader.CanUpgrade( oldMember ) )
					continue;

				upgrader.UpgradeMember( oldInstance, oldUpgradable, newInstance, newUpgradable );
				wasUpgraded = true;

				break;
			}

			if ( !wasUpgraded )
				Log.Warning( $"Don't know how to upgrade {oldMember.MemberType.ToString().ToLower()} '{oldMember.Name}' in '{oldType.Name}'" );
		}
	}
}
