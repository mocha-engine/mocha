global using Mocha;
global using Mocha.Common;
using Mocha.Glue;

namespace Minimal;

public class Game : BaseGame
{
	public override void OnStartup()
	{
		// Spawn a model to walk around in
		var map = new StaticMeshActor( "models/dev/dev_map.mmdl" );
		// map.SetMeshPhysics( "models/dev/dev_map.mmdl" );

		// Spawn a player
		var player = new Player();
		player.Position = new Vector3( 0, 5, 10 );

		_ = new PostProcess( "shaders/tonemap/agx.mshdr" );
	}
}
