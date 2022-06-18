namespace Mocha.Engine;

/// <summary>
/// Handles the creation and management of various systems, including the game
/// window.
/// </summary>
internal class Game
{
	RendererInstance renderer;

	public Game()
	{
		Log.Trace( "Initializing game" );
		renderer = new();
		var world = new World();
		renderer.OnUpdate += world.Update;

		renderer.Run();
	}
}
