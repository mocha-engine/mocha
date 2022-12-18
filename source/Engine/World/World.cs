
using Mocha.UI;

namespace Mocha;

public class World
{
	public static World Current { get; set; }

	public World()
	{
		Current = this;
		Event.Register( this );
		Event.RegisterStatics();
		Event.Run( Event.Game.LoadAttribute.Name );

		SetupEntities();
	}

	private void SetupEntities()
	{
		Log.Trace( $"Setting up entities..." );

		var map = new ModelEntity( "core/models/dev/dev_map.mmdl" );
		map.Position = new( 0.0f, 10.0f, 0.0f );
		map.Friction = 1.0f;
		map.Restitution = 0.0f;
		map.Mass = 1000.0f;
		map.SetMeshPhysics( "core/models/dev/dev_map.mmdl" );

		var ui = new UIManager();

		var player = new Player();
	}

	public void Update()
	{
		UIManager.Instance.Render();
		BaseEntity.All.ToList().ForEach( entity => entity.Update() );
	}
}
