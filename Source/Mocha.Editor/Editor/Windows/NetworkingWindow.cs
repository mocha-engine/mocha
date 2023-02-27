using Mocha.Editor;

[Title( "Networking" )]
public class NetworkingWindow : EditorWindow
{
	public NetworkingWindow()
	{
		isVisible = false;
	}

	public override void Draw()
	{
		if ( ImGuiX.BeginWindow( name: $"Networking", ref isVisible ) )
		{

		}

		ImGui.End();
	}
}
