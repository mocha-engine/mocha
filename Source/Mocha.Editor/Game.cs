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

	public void Startup()
	{
		ImGuiNative.igSetCurrentContext( NativeEngine.GetEditorManager().GetContextPointer() );
	}

	public void Update()
	{
		// Stub
	}
}
