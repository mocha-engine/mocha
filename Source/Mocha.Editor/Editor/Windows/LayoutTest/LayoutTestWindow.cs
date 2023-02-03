namespace Mocha.Editor;

[Icon( FontAwesome.Folder ), Title( "ImGui Test" ), Category( "Engine" )]
internal class ImGuiTestWindow : EditorWindow
{
	public ImGuiTestWindow()
	{
		isVisible = false;
	}

	public override void Draw()
	{
		if ( ImGuiX.BeginWindow( "ImGui Test", ref isVisible ) )
		{
			using ( var layout = new HorizontalLayout( "Horizontal Layout Start" ) )
			{
				layout.Add( ImGui.Button( "Start" ) );
				layout.Add( ImGui.Button( "Hello!!" ) );
			}

			using ( var layout = new HorizontalLayout( "Horizontal Layout Center", LayoutAlignment.Center ) )
			{
				layout.Add( ImGui.Button( "Centered..." ) );
				layout.Add( ImGui.Button( "wow" ) );
			}

			using ( var layout = new HorizontalLayout( "Horizontal Layout End", LayoutAlignment.End ) )
			{
				layout.Add( ImGui.Button( "End" ) );
				layout.Add( ImGui.Button( "Lorem ipsum" ) );
			}

			ImGui.End();
		}
	}
}
