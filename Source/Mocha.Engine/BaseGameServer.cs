using Mocha.Networking;
using System.Collections.Immutable;
using System.Reflection;

namespace Mocha;

internal struct Snapshot
{
	private ImmutableList<IEntity> _entities;

	public static Snapshot Create()
	{
		// Create a snapshot based on everything in EntityRegistry
		var snapshot = new Snapshot();
		snapshot._entities = EntityRegistry.Instance.ToImmutableList();

		return snapshot;
	}

	public static List<MemberInfo> Delta( Snapshot snapshot1, Snapshot snapshot2 )
	{
		var changedMembers = new List<MemberInfo>();

		// Get the list of entities from each snapshot
		var entities1 = snapshot1._entities;
		var entities2 = snapshot2._entities;

		// Loop through each entity in snapshot2
		foreach ( var entity2 in entities2 )
		{
			// Find the corresponding entity in snapshot1, if any
			var entity1 = entities1.FirstOrDefault( e => e.NetworkId == entity2.NetworkId );

			// If the entity doesn't exist in snapshot1, it's a new entity and all its members have changed
			if ( entity1 == null )
			{
				changedMembers.AddRange( entity2.GetType().GetMembers().ToList() );
				continue;
			}

			// Loop through each member of the entity
			foreach ( var member in entity2.GetType().GetMembers() )
			{
				// Skip non-property and non-field members
				if ( member is not PropertyInfo && member is not FieldInfo )
					continue;

				// Get the value of the member for each entity
				var value1 = GetValueForMember( member, entity1 );
				var value2 = GetValueForMember( member, entity2 );

				// Compare the values
				if ( !object.Equals( value1, value2 ) )
				{
					changedMembers.Add( member );
				}
			}
		}

		return changedMembers;
	}

	public SnapshotUpdateMessage CreateSnapshotUpdateMessage()
	{
		// Send initial SnapshotUpdateMessage
		var snapshotUpdateMessage = new SnapshotUpdateMessage
		{
			PreviousTimestamp = -1,
			CurrentTimestamp = Time.Now,
			SequenceNumber = 0
		};

		foreach ( var entity in EntityRegistry.Instance )
		{
			var entityChange = new SnapshotUpdateMessage.EntityChange();
			entityChange.NetworkId = entity.NetworkId;
			entityChange.MemberChanges = new List<SnapshotUpdateMessage.EntityMemberChange>();
			entityChange.TypeName = entity.GetType().FullName!;

			if ( entity.NetworkId.IsLocal() )
				continue; // Not networked, skip

			foreach ( var member in entity.GetType().GetMembers() )
			{
				// Only replicate fields and properties that are marked with [Replicated].
				if ( member.GetCustomAttribute<SyncAttribute>() == null )
					continue;

				if ( member.MemberType == MemberTypes.Property )
				{
					var property = member as PropertyInfo;

					if ( property != null )
					{
						var value = property.GetValue( entity );
						var entityMemberChange = new SnapshotUpdateMessage.EntityMemberChange();
						entityMemberChange.FieldName = property.Name;
						entityMemberChange.Data = NetworkSerializer.Serialize( value );
						entityChange.MemberChanges.Add( entityMemberChange );
					}
				}
				else if ( member.MemberType == MemberTypes.Field )
				{
					var field = member as FieldInfo;

					if ( field != null )
					{
						var value = field.GetValue( entity );
						var entityMemberChange = new SnapshotUpdateMessage.EntityMemberChange();
						entityMemberChange.FieldName = field.Name;
						entityMemberChange.Data = NetworkSerializer.Serialize( value );
						entityChange.MemberChanges.Add( entityMemberChange );
					}
				}
			}

			snapshotUpdateMessage.EntityChanges.Add( entityChange );
		}

		return snapshotUpdateMessage;
	}

	// Helper function to get the value of a member for an entity
	private static object GetValueForMember( MemberInfo member, IEntity entity )
	{
		if ( member is PropertyInfo property )
		{
			return property.GetValue( entity );
		}
		else if ( member is FieldInfo field )
		{
			return field.GetValue( entity );
		}
		else
		{
			throw new ArgumentException( $"Member {member.Name} is not a property or field" );
		}
	}
}

internal class SnapshotList : List<Snapshot>
{
	public SnapshotList( int capacity ) : base( capacity )
	{
	}

	public SnapshotList( IEnumerable<Snapshot> collection ) : base( collection )
	{
	}

	public SnapshotList()
	{
	}

	private new void Add( Snapshot snapshot )
	{
		// Add the snapshot to the list
		base.Add( snapshot );

		// Remove the oldest snapshot if the list is too long
		if ( Count > 32 )
		{
			RemoveAt( 0 );
		}
	}

	public void Update()
	{
		var snapshot = Snapshot.Create();
		Add( snapshot );
	}
}

public class BaseGameServer : Server
{
	private Dictionary<IConnection, SnapshotList> _snapshots = new Dictionary<IConnection, SnapshotList>();

	public Action<IConnection> OnClientConnectedEvent;
	public Action<IConnection> OnClientDisconnectedEvent;

	public BaseGameServer()
	{
		RegisterHandler<ClientInputMessage>( OnClientInputMessage );
	}

	public override void OnClientConnected( IConnection connection )
	{
		if ( connection is not ClientConnection client )
			return;

		Log.Info( $"BaseGameServer: Client {client} connected" );

		// Send initial HandshakeMessage
		var handshakeMessage = new HandshakeMessage
		{
			Timestamp = Time.Now,
			TickRate = Core.TickRate,
			Nickname = client.Nickname
		};
		client.Send( handshakeMessage );

		var snapshot = new Snapshot();
		var snapshotUpdateMessage = snapshot.CreateSnapshotUpdateMessage();
		client.Send( snapshotUpdateMessage );

		OnClientConnectedEvent?.Invoke( connection );
	}

	public override void OnClientDisconnected( IConnection connection )
	{
		Log.Info( $"BaseGameServer: Client {connection} disconnected" );

		OnClientDisconnectedEvent?.Invoke( connection );
	}

	public override void OnMessageReceived( IConnection client, byte[] data )
	{
		InvokeHandler( client, data );
	}

	public void OnClientInputMessage( IConnection client, ClientInputMessage clientInputMessage )
	{
		var snapshot = new Snapshot();
		var snapshotUpdateMessage = snapshot.CreateSnapshotUpdateMessage();
		client.Send( snapshotUpdateMessage );

		return;

		Log.Info( $@"BaseGameServer: Client {client} sent input message:
			ViewAngles: {clientInputMessage.ViewAnglesP}, {clientInputMessage.ViewAnglesY}, {clientInputMessage.ViewAnglesR}
			Direction: {clientInputMessage.DirectionX}, {clientInputMessage.DirectionY}, {clientInputMessage.DirectionZ}
			Left: {clientInputMessage.Left}
			Right: {clientInputMessage.Right}
			Middle: {clientInputMessage.Middle}" );
	}

	public override void OnUpdate()
	{
		foreach ( var connection in _connectedClients )
		{
			SnapshotList list;
			if ( !_snapshots.TryGetValue( connection, out list ) )
				list = _snapshots[connection] = new SnapshotList();

			list.Update();
		}
	}
}
