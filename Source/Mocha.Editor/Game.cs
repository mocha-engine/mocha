namespace Mocha.Editor;

public class Game : IGame
{
	public void FrameUpdate()
	{
		DebugOverlay.Render();
		NotificationOverlay.Render();
		Editor.Draw();
	}

	public void Shutdown()
	{
		// Stub
	}

	public void Startup()
	{
		ImGuiNative.igSetCurrentContext( Glue.Editor.GetContextPointer() );
	}

	public void Update()
	{
		// Stub
	}
}
