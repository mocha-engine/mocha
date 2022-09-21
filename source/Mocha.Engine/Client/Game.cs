namespace Mocha.Engine;

/// <summary>
/// Handles the creation of various game systems.
/// </summary>
internal class Game
{
	private RendererInstance renderer;
	private Editor.Editor editor;

	internal Game()
	{
		if ( Veldrid.RenderDoc.Load( out var renderDoc ) )
		{
			renderDoc.OverlayEnabled = false;
			Log.Trace( "Loaded RenderDoc" );
		}

		using ( var _ = new Stopwatch( "Game init" ) )
		{
			Log.Trace( "Game init" );
			renderer = new();
			editor = new();

			var world = new World();

			// Must be called before everything else
			renderer.PreUpdate += Input.Update;

			renderer.OnUpdate += world.Update;

			// Must be called after everything else
			// renderer.PostUpdate += editor.Update;

			renderer.RenderOverlays += editor.Render;
		}

		renderer.Run();
	}
}
