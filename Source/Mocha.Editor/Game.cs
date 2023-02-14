namespace Mocha.Editor;

public class Game : IGame
{
	public void FrameUpdate()
	{
		DebugOverlay.Render();
		Notifications.Render();
		ConsoleOverlay.Render();
		Editor.Draw();
	}

	public void Shutdown()
	{
		// Stub
	}

	public void Startup( bool isServer )
	{
		ImGuiNative.igSetCurrentContext( NativeEngine.GetEditorManager().GetContextPointer() );
		Core.IsServer = isServer;
		Core.IsClient = !isServer;
	}

	public void Update()
	{
		// Stub
	}
}
