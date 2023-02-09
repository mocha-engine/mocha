global using Mocha;
global using Mocha.Common;

namespace Minimal;

public class Game : BaseGame
{
	[HotloadSkip]
	private UIManager Hud { get; set; }

	public override void Startup()
	{
		// Set up UI
		Hud = new UIManager();
		Hud.SetTemplate( "ui/Game.html" );

		// Spawn a model to walk around in
		var map = new ModelEntity( "models/dev/dev_map.mmdl" );
		map.SetMeshPhysics( "models/dev/dev_map.mmdl" );

		// Spawn a player
		var player = new Player();
	}
}
