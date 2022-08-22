using Mocha.Common.World;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha;

[Title( "Entity" ), Icon( FontAwesome.VectorSquare )]
public class BaseEntity : IEntity
{
	public static List<BaseEntity> All { get; set; } = Assembly.GetCallingAssembly().GetTypes().OfType<BaseEntity>().ToList();

	private Transform transform;

	Transform IEntity.Transform
	{
		get => transform;
		set => transform = value;
	}

	public Vector3 Scale
	{
		get => transform.Scale;
		set => transform.Scale = value;
	}

	public Vector3 Position
	{
		get => transform.Position;
		set => transform.Position = value;
	}

	public Rotation Rotation
	{
		get => transform.Rotation;
		set => transform.Rotation = value;
	}

	[HideInInspector]
	public string Name { get; set; }

	[HideInInspector]
	public int Id { get; set; }

	public BaseEntity()
	{
		Id = All.Count; // TODO: Pooling
		All.Add( this );
		Name = $"{this.GetType().Name} {All.Count}";
	}

	public virtual void Render() { }
	public virtual void Update() { }
	public virtual void Delete()
	{
		Log.Trace( "TODO: Manage entity objects manually" );
	}

	public bool Equals( BaseEntity x, BaseEntity y ) => x.GetHashCode() == y.GetHashCode();
	public int GetHashCode( [DisallowNull] BaseEntity obj ) => base.GetHashCode();

	private int parentId;

	[HideInInspector]
	public BaseEntity Parent => BaseEntity.All.First( x => x.Id == parentId );

	[HideInInspector]
	public List<BaseEntity> Children => BaseEntity.All.Where( x => x.parentId == Id ).ToList();

	[HideInInspector]
	public bool Visible { get; set; } = true;

	public void SetParent( BaseEntity newParent )
	{
		newParent.parentId = newParent.Id;
	}
}
