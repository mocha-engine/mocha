using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha.Engine;

public class Entity : IEntity
{
	public static List<Entity> All { get; set; } = Assembly.GetCallingAssembly().GetTypes().OfType<Entity>().ToList();

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

	public string Name { get; set; }
	public int Id { get; set; }

	public Entity()
	{
		Id = All.Count; // TODO: Pooling
		All.Add( this );
		Name = $"{this.GetType().Name} {All.Count}";
	}

	public virtual void Update() { }
	public virtual void Delete() { }

	public bool Equals( Entity x, Entity y ) => x.GetHashCode() == y.GetHashCode();
	public int GetHashCode( [DisallowNull] Entity obj ) => base.GetHashCode();

	private int parentId;
	public Entity Parent => Entity.All.First( x => x.Id == parentId );
	public List<Entity> Children => Entity.All.Where( x => x.parentId == Id ).ToList();

	public void SetParent( Entity newParent )
	{
		newParent.parentId = newParent.Id;
	}
}
