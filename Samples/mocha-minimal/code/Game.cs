using Mocha;
using Mocha.UI;

namespace Minimal;

public class Game : BaseGame
{
	private UIManager Hud { get; set; }

	public override void Startup()
	{
		// Set up UI
		Hud = new UIManager();
		Hud.SetTemplate( "ui/Game.html" );

		// Spawn a model to walk around in
		var map = new ModelEntity( "core/models/dev/dev_map.mmdl" );
		map.SetMeshPhysics( "core/models/dev/dev_map.mmdl" );

		// Spawn a player
		var player = new Player();
	}
}
