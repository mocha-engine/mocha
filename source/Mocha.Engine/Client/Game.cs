namespace Mocha.Engine;

/// <summary>
/// Handles the creation of various game systems.
/// </summary>
internal class Game
{
	private RendererInstance renderer;

#if DEBUG
	private Editor editor;
#endif

	internal Game()
	{
		if ( Veldrid.RenderDoc.Load( out var renderDoc ) )
		{
			renderDoc.OverlayEnabled = false;
			// renderDoc.RefAllResources = false;
			Log.Trace( "Loaded RenderDoc" );
		}

		using ( var _ = new Stopwatch( "Game init" ) )
		{
			Log.Trace( "Game init" );
			renderer = new();

			var remoteConsole = new RemoteConsoleServer();
			Logger.OnLog += ( level, str, stackTrace ) =>
			{
				remoteConsole.Write(
					level,
					str,
					stackTrace.GetFrame( 2 ).GetMethod().DeclaringType.Name,
					stackTrace.GetFrames().Select( x => x.ToString() ).ToArray() );
			};

#if DEBUG
			editor = new( renderer );
#endif

			var world = new World();

			// Must be called before everything else
			renderer.PreUpdate += Input.Update;

			renderer.OnUpdate += world.Update;

#if DEBUG
			// Must be called after everything else
			renderer.PostUpdate += editor.Update;
#endif
		}

		renderer.Run();
	}
}
