﻿using Mocha.Networking;
using System.Reflection;

namespace Mocha;
public class BaseGameClient : Client
{
	private ServerConnection _connection;

	public BaseGameClient( string ipAddress, ushort port = 10570 ) : base( ipAddress, port )
	{
		_connection = new ServerConnection();
		RegisterHandler<KickedMessage>( OnKickedMessage );
		RegisterHandler<SnapshotUpdateMessage>( OnSnapshotUpdateMessage );
		RegisterHandler<HandshakeMessage>( OnHandshakeMessage );
	}

	public override void OnMessageReceived( byte[] data )
	{
		InvokeHandler( _connection, data );
	}

	public void OnKickedMessage( IConnection connection, KickedMessage kickedMessage )
	{
		Log.Info( $"BaseGameClient: We were kicked: '{kickedMessage.Reason}'" );
	}

	private Type? LocateType( string typeName )
	{
		var type = Type.GetType( typeName )!;

		if ( type != null )
			return type;

		type = Assembly.GetExecutingAssembly().GetType( typeName );

		if ( type != null )
			return type;

		type = Assembly.GetCallingAssembly().GetType( typeName );

		if ( type != null )
			return type;

		foreach ( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
		{
			type = assembly.GetType( typeName );
			if ( type != null )
				return type;
		}

		return null;
	}

	public void OnSnapshotUpdateMessage( IConnection connection, SnapshotUpdateMessage snapshotUpdateMessage )
	{
		foreach ( var entityChange in snapshotUpdateMessage.EntityChanges )
		{
			Log.Info( $"BaseGameClient: Entity {entityChange.NetworkId} changed" );

			// Does this entity already exist?
			var entity = EntityRegistry.Instance.FirstOrDefault( x => x.NetworkId == entityChange.NetworkId );

			if ( entity == null )
			{
				// Entity doesn't exist locally - let's create it
				var type = LocateType( entityChange.TypeName );

				if ( type == null )
				{
					Log.Error( $"BaseGameClient: Unable to locate type '{entityChange.TypeName}'" );
					continue;
				}

				entity = (Activator.CreateInstance( type ) as IEntity)!;

				// Set the network ID
				entity.NetworkId = entityChange.NetworkId;

				Log.Info( $"BaseGameClient: Created entity {entity.NetworkId}" );
			}

			foreach ( var fieldChange in entityChange.FieldChanges )
			{
				var member = entity.GetType().GetMember( fieldChange.FieldName ).First()!;
				if ( member.MemberType == MemberTypes.Field )
				{
					var field = (FieldInfo)member;
					field.SetValue( entity, fieldChange.Value );

					Log.Info( $"BaseGameClient: Entity {entityChange.NetworkId} field {fieldChange.FieldName} changed to {fieldChange.Value}" );
				}
				else if ( member.MemberType == MemberTypes.Property )
				{
					var property = (PropertyInfo)member;
					property.SetValue( entity, fieldChange.Value );

					Log.Info( $"BaseGameClient: Entity {entityChange.NetworkId} property {fieldChange.FieldName} changed to {fieldChange.Value}" );
				}
			}
		}
	}

	public void OnHandshakeMessage( IConnection connection, HandshakeMessage handshakeMessage )
	{
		Log.Info( $"BaseGameClient: Handshake received. Tick rate: {handshakeMessage.TickRate}, nickname: {handshakeMessage.Nickname}" );
	}
}
