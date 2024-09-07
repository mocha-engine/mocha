global using Mocha;
global using Mocha.Common;

namespace Minimal;

public class Game : BaseGame
{
	[HotloadSkip] private UIManager Hud { get; set; }

	public string NetworkedString { get; set; }

	public override void OnStartup()
	{
		// Spawn a model to walk around in
		var map = new ModelEntity( "models/dev/dev_map.mmdl" );
		map.SetMeshPhysics( "models/dev/dev_map.mmdl" );

		// Spawn a player
		var player = new Player();
		player.Position = new Vector3( 0, 0, 50 );

		Hud = new UIManager();
		Hud.SetTemplate( "ui/Game.html" );
	}

	[Event.Tick]
	public void Tick()
	{
		DebugOverlay.ScreenText( $"Tick... ({GetType().Assembly.GetHashCode()})" );
	}
}
