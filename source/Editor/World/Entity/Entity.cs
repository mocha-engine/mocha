using Mocha.Common.World;
using System.Diagnostics.CodeAnalysis;

namespace Mocha.Editor;

[Title( "Entity" ), Icon( FontAwesome.VectorSquare )]
public class Entity : IEntity
{
	public static IReadOnlyList<Entity> All => EntityPool.Entities.OfType<Entity>().ToList().AsReadOnly();

	private Transform transform;

	public Transform Transform
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

	public string Name { get; set; }

	public int Id { get; set; }

	public Entity()
	{
		Id = EntityPool.RegisterEntity( this );
		Name = $"{this.GetType().Name} {Id}";
	}

	public virtual void Update() { }
	public virtual void Delete( bool immediate = true )
	{
		EntityPool.DeleteEntity( this );

		if ( immediate )
			GC.Collect();
	}

	public bool Equals( Entity x, Entity y ) => x.GetHashCode() == y.GetHashCode();
	public int GetHashCode( [DisallowNull] Entity obj ) => base.GetHashCode();

	private int parentId;

	public Entity Parent => Entity.All.First( x => x.Id == parentId );

	public List<Entity> Children => Entity.All.Where( x => x.parentId == Id ).ToList();

	public void SetParent( Entity newParent )
	{
		newParent.parentId = newParent.Id;
	}

	public virtual void BuildCamera( ref CameraSetup cameraSetup )
	{
	}
}
