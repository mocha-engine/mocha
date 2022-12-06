using Mocha.Common.World;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha;

[Title( "Entity" ), Icon( FontAwesome.VectorSquare )]
public class BaseEntity : IEntity
{
	protected uint NativeHandle { get; set; }

	[Obsolete]
	public static List<BaseEntity> All { get; set; } = Assembly.GetCallingAssembly().GetTypes().OfType<BaseEntity>().ToList();

	[Obsolete]
	private Transform transform;

	[Obsolete]
	Transform IEntity.Transform
	{
		get => transform;
		set => transform = value;
	}

	public Vector3 Scale
	{
		get => Glue.Entities.GetScale( NativeHandle );
		set => Glue.Entities.SetScale( NativeHandle, value );
	}

	public Vector3 Position
	{
		get => Glue.Entities.GetPosition( NativeHandle );
		set => Glue.Entities.SetPosition( NativeHandle, value );
	}

	[Obsolete]
	public Rotation Rotation
	{
		get;
		set;
	}

	[HideInInspector]
	public string Name
	{
		get => Glue.Entities.GetName( NativeHandle );
		set => Glue.Entities.SetName( NativeHandle, value );
	}

	[HideInInspector, Obsolete]
	public int Id { get; set; }

	public BaseEntity()
	{
		Id = All.Count; // TODO: Pooling
		All.Add( this );

		CreateNativeEntity();
		Spawn();
	}

	protected virtual void Spawn()
	{
		Name = $"{GetType().Name} {All.Count}";
	}

	protected virtual void CreateNativeEntity()
	{
		NativeHandle = Glue.Entities.CreateBaseEntity();
	}

	public virtual void Render() { }
	public virtual void Update() { }
	public virtual void Delete()
	{
		Log.Trace( "TODO: Manage entity objects manually" );
	}

	public bool Equals( BaseEntity x, BaseEntity y ) => x.GetHashCode() == y.GetHashCode();
	public int GetHashCode( [DisallowNull] BaseEntity obj ) => base.GetHashCode();

	[Obsolete]
	private int parentId;

	[Obsolete, HideInInspector]
	public BaseEntity Parent => BaseEntity.All.First( x => x.Id == parentId );

	[Obsolete, HideInInspector]
	public List<BaseEntity> Children => BaseEntity.All.Where( x => x.parentId == Id ).ToList();

	[Obsolete, HideInInspector]
	public bool Visible { get; set; } = true;

	[Obsolete]
	public void SetParent( BaseEntity newParent )
	{
		newParent.parentId = newParent.Id;
	}

	[Obsolete]
	public void Delete( bool immediate )
	{
	}
}
