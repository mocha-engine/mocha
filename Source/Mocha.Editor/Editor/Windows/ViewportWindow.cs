using Mocha.Editor;

[Title( "Console" )]
public class ViewportWindow : EditorWindow
{
	public override void Draw()
	{
		if ( ImGuiX.BeginWindow( name: $"Viewport", ref isVisible ) )
		{
			ImGui.Text( "Pretend you can see something here" );
		}

		ImGui.End();
	}
}
