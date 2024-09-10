using System.Diagnostics.CodeAnalysis;

namespace Mocha;

[Title( "Entity" ), Icon( FontAwesome.VectorSquare )]
public class Actor : IActor
{
	public Vector3 Position
	{
		get => Transform.Position;
		set => Transform = Transform.WithPosition( value );
	}

	public Rotation Rotation
	{
		get => Transform.Rotation;
		set => Transform = Transform.WithRotation( value );
	}

	public Vector3 Scale
	{
		get => Transform.Scale;
		set => Transform = Transform.WithScale( value );
	}

	public static List<Actor> All => EntityRegistry.Instance.OfType<Actor>().ToList();

	public Transform Transform { get; set; }

	[HideInInspector]
	public string Name { get; set; }

	public Actor()
	{
		EntityRegistry.Instance.RegisterEntity( this );

		Transform = Transform.Default;

		var displayInfo = DisplayInfo.For( this );
		Name = $"[{displayInfo.Category}] {displayInfo.Name}";

		Event.Register( this );
		Log.Info( $"Spawning entity {Name} on {(Core.IsServer ? "client" : "server")}" );

		Spawn();
	}

	protected virtual void Spawn() { }
	public virtual void Update() { }
	public virtual void FrameUpdate() { }

	public virtual void Delete()
	{
		All.Remove( this );
	}

	public bool Equals( Actor x, Actor y ) => x.GetHashCode() == y.GetHashCode();
	public int GetHashCode( [DisallowNull] Actor obj ) => base.GetHashCode();
}
