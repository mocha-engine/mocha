global using static Mocha.Common.Global;

using Mocha;
using Mocha.UI;

namespace Minimal;

public class Game : BaseGame
{
	private UIManager ui;

	public override void Startup()
	{
		ui = new UIManager();
		ui.SetTemplate( "ui/Game.html" );
		SetupEntities();
	}

	private void SetupEntities()
	{
		Log.Trace( $"Setting up entities" );

		var map = new ModelEntity( "core/models/dev/dev_map.mmdl" );
		map.Position = new( 0.0f, 0.0f, 0.0f );
		map.Friction = 1.0f;
		map.Restitution = 0.0f;
		map.Mass = 1000.0f;
		map.SetMeshPhysics( "core/models/dev/dev_map.mmdl" );

		var player = new Player();
	}

	public override void Update()
	{
		base.Update();

		Log.Trace( "Poo" );
	}
}
