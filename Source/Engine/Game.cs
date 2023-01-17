using Mocha.UI;

namespace Mocha;

public class Game : IGame
{
	public static Game Current { get; set; }

	private UIManager ui;

	public void Startup()
	{
		Current = this;
		Event.Register( this );
		Event.RegisterStatics();
		Event.Run( Event.Game.LoadAttribute.Name );

		ui = new UIManager();
		ui.SetTemplate( "ui/Game.html" );
		SetupEntities();
	}

	private void SetupEntities()
	{
		Log.Trace( $"Setting up entities..." );

		var map = new ModelEntity( "core/models/dev/dev_map.mmdl" );
		map.Position = new( 0.0f, 0.0f, 0.0f );
		map.Friction = 1.0f;
		map.Restitution = 0.0f;
		map.Mass = 1000.0f;
		map.SetMeshPhysics( "core/models/dev/dev_map.mmdl" );

		var player = new Player();
	}

	public void Update()
	{
		BaseEntity.All.ToList().ForEach( entity => entity.Update() );
	}

	public void FrameUpdate()
	{
		UIManager.Instance.Render();
		BaseEntity.All.ToList().ForEach( entity => entity.FrameUpdate() );
	}

	public void Shutdown()
	{
	}
}
