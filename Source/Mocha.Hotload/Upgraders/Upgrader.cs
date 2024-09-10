using Mocha.Common;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Mocha.Hotload.Upgrading;

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

	/// <summary>
	/// An array of all the upgrader instances to use.
	/// </summary>
	private static ImmutableArray<IMemberUpgrader> s_upgraders { get; set; } = ImmutableArray<IMemberUpgrader>.Empty;

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
			.Where( t => t.GetInterface( nameof( IMemberUpgrader ) ) is not null );

		var upgraders = ImmutableArray.CreateBuilder<IMemberUpgrader>();
		foreach ( var upgraderType in upgraderTypes )
			upgraders.Add( (IMemberUpgrader)Activator.CreateInstance( upgraderType )! );
		s_upgraders = upgraders.OrderByDescending( upgrader => upgrader.Priority ).ToImmutableArray();
	}

	/// <summary>
	/// Upgrades a given <see cref="Assembly"/> and its entry point.
	/// </summary>
	/// <typeparam name="T">The type of the entry points.</typeparam>
	/// <param name="oldAssembly">The old <see cref="Assembly"/> that is being unloaded.</param>
	/// <param name="newAssembly">The new <see cref="Assembly"/> that is being loaded.</param>
	/// <param name="oldEntryPoint">The entry point that was created from the <see ref="oldAssembly"/>.</param>
	/// <param name="newEntryPoint">The entry point that was created from the <see ref="newAssembly"/>.</param>
	internal static void Upgrade<T>( Assembly oldAssembly, Assembly newAssembly, T oldEntryPoint, T newEntryPoint )
	{
		UpgradedReferences.Clear();

		// Upgrade static members.
		foreach ( var oldType in oldAssembly.GetTypes() )
		{
			var newType = newAssembly.GetType( oldType.FullName ?? oldType.Name );
			if ( newType is null )
				continue;

			UpgradeStaticInstance( oldType, newType );
		}

		// Upgrade entity types.
		UpgradeEntities( oldAssembly, newAssembly );
		// Upgrade entry point.
		UpgradeInstance( oldEntryPoint, newEntryPoint );
	}

	/// <summary>
	/// Upgrades all entities that were affected by the swap.
	/// </summary>
	/// <param name="oldAssembly">The old assembly being unloaded.</param>
	/// <param name="newAssembly">The new assembly being loaded.</param>
	internal static void UpgradeEntities( Assembly oldAssembly, Assembly newAssembly )
	{
		var entityRegistryCopy = EntityRegistry.Instance.ToList();

		for ( int i = 0; i < entityRegistryCopy.Count; i++ )
		{
			var entity = entityRegistryCopy[i];
			var entityType = entity.GetType();

			// Do we actually want to upgrade this? If not, skip.
			if ( entityType.Assembly != oldAssembly )
				continue;

			// Unregister the old entity
			EntityRegistry.Instance.UnregisterEntity( entity );

			// Find new type for entity in new assembly
			var newType = newAssembly.GetType( entityType.FullName ?? entityType.Name )!;
			var newEntity = (IActor)FormatterServices.GetUninitializedObject( newType )!;

			// Have we already upgraded this?
			if ( UpgradedReferences.TryGetValue( entity.GetHashCode(), out var upgradedValue ) )
			{
				newEntity = (IActor)upgradedValue;
			}
			else
			{
				UpgradedReferences[entity.GetHashCode()] = newEntity;
				UpgradeInstance( entity, newEntity );
			}

			// If we created a new entity successfully, register it
			if ( newEntity is not null )
				EntityRegistry.Instance.RegisterEntity( newEntity );
		}
	}

	/// <summary>
	/// Upgrades a <see cref="Type"/>s static members.
	/// </summary>
	/// <param name="oldType">The old version of the <see cref="Type"/>.</param>
	/// <param name="newType">The new version of the <see cref="Type"/>.</param>
	internal static void UpgradeStaticInstance( Type oldType, Type newType )
	{
		UpgradeMembers( oldType, newType, null, null );
	}

	/// <summary>
	/// Upgrades an instance of an object.
	/// </summary>
	/// <param name="oldInstance">The old instance.</param>
	/// <param name="newInstance">The new instance.</param>
	internal static void UpgradeInstance( object? oldInstance, object? newInstance )
	{
		// Bail
		if ( oldInstance is null || newInstance is null )
			return;

		// Unregister events for old object
		Event.Unregister( oldInstance );

		// Upgrade the members.
		UpgradeMembers( oldInstance.GetType(), newInstance.GetType(), oldInstance, newInstance );

		// Register events for new object
		Event.Register( newInstance );
	}

	/// <summary>
	/// Upgrades all members on a type.
	/// </summary>
	/// <param name="oldType">The old version of the <see cref="Type"/>.</param>
	/// <param name="newType">The new version of the <see cref="Type"/>.</param>
	/// <param name="oldInstance">The old instance.</param>
	/// <param name="newInstance">The new instance.</param>
	private static void UpgradeMembers( Type oldType, Type newType, object? oldInstance, object? newInstance )
	{
		// If both instance are null then we're upgrading static members.
		var bindingFlags = (oldInstance is null && newInstance is null)
			? BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
			: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		// Get all members from the old type
		var oldMembers = oldType.GetMembers( bindingFlags );

		// For each member:
		// - If it's a reference type, we will want to upgrade it if it's a class
		//   and not a delegate
		// - Otherwise, copy the value
		foreach ( var oldMember in oldMembers )
		{
			//
			// Old member
			//
			if ( oldMember.GetCustomAttribute<CompilerGeneratedAttribute>() is not null ||
				oldMember.GetCustomAttribute<HotloadSkipAttribute>() is not null )
				continue;

			var oldUpgradable = UpgradableMember.FromMember( oldMember );
			// Can we upgrade this?
			if ( oldUpgradable is null )
				continue;

			//
			// New member
			//
			var newMember = newType.GetMember( oldMember.Name, bindingFlags ).FirstOrDefault();
			// Does this member exist? (eg. might have been deleted)
			if ( newMember is null || newMember.GetCustomAttribute<HotloadSkipAttribute>() is not null )
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
