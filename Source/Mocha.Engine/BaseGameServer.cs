using Mocha.Networking;
using System.Reflection;

namespace Mocha;
public class BaseGameServer : Server
{
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
		var handshakeMessage = new HandshakeMessage();
		handshakeMessage.TickRate = Core.TickRate;
		handshakeMessage.Nickname = client.Nickname;
		client.Send( handshakeMessage );

		// Send initial SnapshotUpdateMessage
		var snapshotUpdateMessage = new SnapshotUpdateMessage();
		snapshotUpdateMessage.PreviousTimestamp = 0;
		snapshotUpdateMessage.CurrentTimestamp = 0;
		snapshotUpdateMessage.SequenceNumber = 0;

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
				if ( member.GetCustomAttribute<ReplicatedAttribute>() == null )
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

		client.Send( snapshotUpdateMessage );
	}

	public override void OnClientDisconnected( IConnection client )
	{
		Log.Info( $"BaseGameServer: Client {client} disconnected" );
	}

	public override void OnMessageReceived( IConnection client, byte[] data )
	{
		InvokeHandler( client, data );
	}

	public void OnClientInputMessage( IConnection client, ClientInputMessage clientInputMessage )
	{
		return;

		Log.Info( $@"BaseGameServer: Client {client} sent input message:
			ViewAngles: {clientInputMessage.ViewAnglesP}, {clientInputMessage.ViewAnglesY}, {clientInputMessage.ViewAnglesR}
			Direction: {clientInputMessage.DirectionX}, {clientInputMessage.DirectionY}, {clientInputMessage.DirectionZ}
			Left: {clientInputMessage.Left}
			Right: {clientInputMessage.Right}
			Middle: {clientInputMessage.Middle}" );
	}
}
