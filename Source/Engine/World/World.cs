using Mocha.UI;

namespace Mocha;

public class World
{
	public static World Current { get; set; }
	private bool worldInitialized = false;
	private bool frameRendered = false;

	private UIManager ui;

	public World()
	{
		Current = this;
		Event.Register( this );
		Event.RegisterStatics();
		Event.Run( Event.Game.LoadAttribute.Name );

		ui = new UIManager();
		ui.SetTemplate( "ui/Loading.html" );
		// SetupEntities();
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

		ui.SetTemplate( "ui/Game.html" );
	}

	public void Render()
	{
		DebugOverlay.Render();
		UIManager.Instance.Render();

		if ( !worldInitialized )
		{
			SetupEntities();

			worldInitialized = true;
		}
	}

	private void DrawDebug()
	{
		// TODO: Remove
		int ticksPerSecond = (1.0f / Glue.Engine.GetTickDeltaTime()).CeilToInt();
		int framesPerSecond = (1.0f / Glue.Engine.GetDeltaTime()).CeilToInt();

		DebugOverlay.ScreenText( $"Ticks per second: {ticksPerSecond}" );
		DebugOverlay.ScreenText( $"Frames per second: {framesPerSecond}" );
		DebugOverlay.ScreenText( $"Current tick: {Glue.Engine.GetCurrentTick()}" );
		DebugOverlay.ScreenText( $"Current time: {Glue.Engine.GetTime()}" );
		DebugOverlay.ScreenText( $"Current tick time: {Glue.Engine.GetTickDeltaTime()}ms" );
		DebugOverlay.ScreenText( $"Current frame time: {Glue.Engine.GetDeltaTime()}ms" );
		DebugOverlay.ScreenText( $"Time.Now: {Time.Now}" );
		DebugOverlay.ScreenText( $"Time.Delta: {Time.Delta}ms" );
		DebugOverlay.ScreenText( $"Time.FPS: {Time.FPS}" );
	}

	public void Update()
	{
		DebugOverlay.ScreenText( $"--- Update ---" );
		DrawDebug();

		// TODO: Entity interpolation
		BaseEntity.All.ToList().ForEach( entity => entity.Update() );
	}

	public void FrameUpdate()
	{
		DebugOverlay.ScreenText( $"--- FrameUpdate ---" );
		DrawDebug();

		BaseEntity.All.ToList().ForEach( entity => entity.FrameUpdate() );
	}
}
