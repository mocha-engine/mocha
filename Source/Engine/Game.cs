using Mocha.AssetCompiler;
using VConsoleLib;

namespace Mocha;

public class Game
{
	private World world;
	private VConsoleServer vconsoleServer;

	private void InitFileSystem()
	{
		FileSystem.Game = new FileSystem( "content\\" );
		FileSystem.Game.AssetCompiler = new RuntimeAssetCompiler();
	}

	private void InitVConsole()
	{
		vconsoleServer = new();
		Log.OnLog += ( s ) => vconsoleServer.Log( s );

		vconsoleServer.OnCommand += ( s ) =>
		{
			Log.Info( $"Command: {s}" );
		};
	}

	private void InitImGui()
	{
		ImGuiNative.igSetCurrentContext( Glue.Editor.GetContextPointer() );
	}

	private void InitGame()
	{
		world = new World();
	}

	public void Startup()
	{
		InitFileSystem();
		InitVConsole();
		InitImGui();
		InitGame();
	}

	public void Update()
	{
		Time.UpdateFrom( Glue.Engine.GetTickDeltaTime() );

		world.Update();
	}

	public void Render()
	{
		Time.UpdateFrom( Glue.Engine.GetDeltaTime() );
		Screen.UpdateFrom( Glue.Editor.GetRenderSize() );
		Input.Update();

		world.Render();
		world.FrameUpdate();
	}

	public void DrawEditor()
	{
		Editor.Editor.Draw();
	}
}
