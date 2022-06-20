using VConsoleLib;

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

			var remoteConsole = new RemoteConsoleServer();
			Logger.OnLog += ( level, str, stackTrace ) =>
			{
				remoteConsole.Write( level, str, stackTrace );
			};

			var vconsole = new VConsoleServer();
			Logger.OnLog += ( level, str, _ ) =>
			{
				uint color = 0xFFFFFFFF;
				switch ( level )
				{
					case Logger.Level.Trace:
						color = 0xFFAAAAAA;
						break;
					case Logger.Level.Info:
						color = 0xFFFFFFFF;
						break;
					case Logger.Level.Warning:
						color = 0xAAAAAAFF;
						break;
					case Logger.Level.Error:
						color = 0xFF0000FF;
						break;
				}

				vconsole.Log( str, color );
			};

			vconsole.OnCommand += ( command ) =>
			{
				Log.Error( $"Unknown command '{command}'" );
			};

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
