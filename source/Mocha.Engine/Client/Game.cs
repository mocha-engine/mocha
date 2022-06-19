namespace Mocha.Engine;

/// <summary>
/// Handles the creation of various game systems.
/// </summary>
internal class Game
{
	private RendererInstance renderer;
	private Editor editor;

	internal Game()
	{
		using ( var _ = new Stopwatch( "Game init" ) )
		{
			Log.Trace( "Game init" );
			renderer = new();

			var editorFontTexture = Editor.GenerateFontTexture();
			editor = new( renderer.GetImGuiBinding( editorFontTexture ) );

			var world = new World();

			renderer.PreUpdate += Input.Update; // Must be called before everything else
			renderer.OnUpdate += world.Update;
			renderer.PostUpdate += editor.Update; // Must be called after everything else
		}

		renderer.Run();
	}
}
