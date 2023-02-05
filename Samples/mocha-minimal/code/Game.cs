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

	// This runs on client, and is stripped from the server dll
	[ClientOnly, Event.Tick]
	public void ClientUpdate()
	{
		Log.Trace( "Hello from client" );
	}

	// This runs on server, and is stripped from the client dll
	[ServerOnly, Event.Tick]
	public void ServerUpdate()
	{
		Log.Trace( "Hello from server" );
	}

	// This runs on both client & server
	[Event.Tick]
	public void PredictedUpdate()
	{
		Log.Trace( "Hello from either client or server" );
	}
}
