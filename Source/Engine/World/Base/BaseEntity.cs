using System.Diagnostics.CodeAnalysis;

namespace Mocha;

[Title( "Entity" ), Icon( FontAwesome.VectorSquare )]
public class BaseEntity : IEntity
{
	public static List<BaseEntity> All { get; set; } = new();

	[HideInInspector]
	public uint NativeHandle { get; protected set; }

	public bool IsValid()
	{
		return true;
	}

	[Category( "Transform" )]
	public Vector3 Scale
	{
		get => Glue.Entities.GetScale( NativeHandle );
		set => Glue.Entities.SetScale( NativeHandle, value );
	}

	[Category( "Transform" )]
	public Vector3 Position
	{
		get => Glue.Entities.GetPosition( NativeHandle );
		set => Glue.Entities.SetPosition( NativeHandle, value );
	}

	[Category( "Transform" )]
	public Rotation Rotation
	{
		get => Glue.Entities.GetRotation( NativeHandle );
		set => Glue.Entities.SetRotation( NativeHandle, value );
	}

	[HideInInspector]
	public string Name
	{
		get => Glue.Entities.GetName( NativeHandle );
		set => Glue.Entities.SetName( NativeHandle, value );
	}

	public bool IsViewModel
	{
		set => Glue.Entities.SetViewmodel( NativeHandle, value );
	}

	public bool IsUI
	{
		set => Glue.Entities.SetUI( NativeHandle, value );
	}

	public BaseEntity()
	{
		All.Add( this );

		CreateNativeEntity();

		Position = new Vector3( 0, 0, 0 );
		Rotation = new Rotation( 0, 0, 0, 1 );
		Scale = new Vector3( 1, 1, 1 );

		var displayInfo = DisplayInfo.For( this );
		Name = $"[{displayInfo.Category}] {displayInfo.Name} {NativeHandle}";

		Spawn();
	}

	protected virtual void Spawn()
	{
	}

	protected virtual void CreateNativeEntity()
	{
		NativeHandle = Glue.Entities.CreateBaseEntity();
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
