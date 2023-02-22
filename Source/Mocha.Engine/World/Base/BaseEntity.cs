using System.Diagnostics.CodeAnalysis;

namespace Mocha;

[Title( "Entity" ), Icon( FontAwesome.VectorSquare )]
public class BaseEntity : IEntity
{
	public static List<BaseEntity> All => EntityRegistry.Instance.OfType<BaseEntity>().ToList();

	[HideInInspector]
	public uint NativeHandle { get; protected set; }

	[HideInInspector]
	private Glue.BaseEntity NativeEntity => NativeEngine.GetEntityManager().GetBaseEntity( NativeHandle );

	public bool IsValid()
	{
		return true;
	}

	[Category( "Transform" )]
	public Vector3 Scale
	{
		get => NativeEntity.GetScale();
		set => NativeEntity.SetScale( value );
	}

	[Category( "Transform" )]
	public Vector3 Position
	{
		get => NativeEntity.GetPosition();
		set => NativeEntity.SetPosition( value );
	}

	[Category( "Transform" )]
	public Rotation Rotation
	{
		get => NativeEntity.GetRotation();
		set => NativeEntity.SetRotation( value );
	}

	[HideInInspector]
	public string Name
	{
		get => NativeEntity.GetName();
		set => NativeEntity.SetName( value );
	}

	public bool IsViewModel
	{
		set => NativeEntity.SetViewmodel( value );
	}

	public bool IsUI
	{
		set => NativeEntity.SetUI( value );
	}

	public NetworkId NetworkId { get; set; }

	public BaseEntity()
	{
		EntityRegistry.Instance.RegisterEntity( this );

		CreateNativeEntity();
		CreateNetworkId();

		Position = new Vector3( 0, 0, 0 );
		Rotation = new Rotation( 0, 0, 0, 1 );
		Scale = new Vector3( 1, 1, 1 );

		var displayInfo = DisplayInfo.For( this );
		Name = $"[{displayInfo.Category}] {displayInfo.Name} {NativeHandle}";

		Event.Register( this );
		Log.Info( $"Spawning entity {Name} on {(Core.IsClient ? "client" : "server")}" );

		Spawn();
	}

	private void CreateNetworkId()
	{
		if ( Core.IsClient )
		{
			// On client - we don't want to "upstream" this to the server, so we'll
			// make this a local entity
			NetworkId = NetworkId.CreateLocal();

			Log.Info( $"Created local entity {Name} with network id {NetworkId}" );
		}
		else
		{
			// On server - we'll network this across to clients
			NetworkId = NetworkId.CreateNetworked();

			Log.Info( $"Created networked entity {Name} with network id {NetworkId}" );
		}
	}

	protected virtual void Spawn()
	{
	}

	protected virtual void CreateNativeEntity()
	{
		NativeHandle = NativeEngine.CreateBaseEntity();
	}

	public virtual void Update() { }
	public virtual void FrameUpdate() { }

	public virtual void Delete()
	{
		Log.Trace( "TODO: Manage entity objects manually" );
	}

	public bool Equals( BaseEntity x, BaseEntity y ) => x.GetHashCode() == y.GetHashCode();
	public int GetHashCode( [DisallowNull] BaseEntity obj ) => base.GetHashCode();

	public void Delete( bool immediate )
	{
		// TODO
	}
}
