using System.Diagnostics.CodeAnalysis;

namespace Mocha;

[Title( "Entity" ), Icon( FontAwesome.VectorSquare )]
public class BaseEntity : IEntity
{
	public static List<BaseEntity> All => EntityRegistry.Instance.OfType<BaseEntity>().ToList();

	[HideInInspector]
	public uint NativeHandle { get; protected set; }

	[HideInInspector]
	private Glue.BaseEntity NativeEntity => Engine.GetEntityManager().GetBaseEntity( NativeHandle );

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

	public BaseEntity()
	{
		EntityRegistry.Instance.RegisterEntity( this );

		CreateNativeEntity();

		Position = new Vector3( 0, 0, 0 );
		Rotation = new Rotation( 0, 0, 0, 1 );
		Scale = new Vector3( 1, 1, 1 );

		var displayInfo = DisplayInfo.For( this );
		Name = $"[{displayInfo.Category}] {displayInfo.Name} {NativeHandle}";

		Event.Register( this );

		Spawn();
	}

	protected virtual void Spawn()
	{
	}

	protected virtual void CreateNativeEntity()
	{
		NativeHandle = Engine.CreateBaseEntity();
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
