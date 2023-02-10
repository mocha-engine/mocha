using Mocha.Editor;

[Title( "Networking" )]
public class NetworkingWindow : EditorWindow
{
	public override void Draw()
	{
		if ( ImGuiX.BeginWindow( name: $"Networking", ref isVisible ) )
		{
			if ( ImGui.Button( "Create Listen Server" ) )
			{
				Networking.CreateListenServer();
			}
		}

		ImGui.End();
	}
}
