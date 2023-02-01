namespace Mocha.Editor;

[Icon( FontAwesome.Folder ), Title( "Project Creator" ), Category( "Engine" )]
internal class ProjectCreatorWindow : EditorWindow
{
	private List<string> _projects = new();

	public ProjectCreatorWindow()
	{
		_projects = new List<string>
		{
			"Mocha-minimal",
			"Sponza",
			"MyCoolGame"
		};
	}

	private void DrawProject( string project )
	{
		bool selected = false;
		ImGuiX.Icon( project, "textures/placeholder.mtex", Vector4.One, ref selected );

		ImGui.SameLine();
	}

	public override void Draw()
	{
		if ( ImGuiX.BeginWindow( "Project Browser", ref isVisible ) )
		{
			var windowSize = ImGui.GetWindowSize();

			ImGuiX.Title( $"{FontAwesome.Folder} Your Projects", "Here's a list of your projects. Click one to load it, or click 'New' to make a new one.", drawSubpanel: true );

			ImGuiX.Separator();

			if ( ImGui.BeginChild( "##projects_list", new Vector2( -1, -52 ), false, ImGuiWindowFlags.AlwaysUseWindowPadding ) )
			{
				ImGuiX.BumpCursorX( 8 );
				ImGuiX.BumpCursorY( 8 );

				foreach ( var project in _projects )
				{
					DrawProject( project );
				}

				DrawProject( "New..." );
			}

			ImGui.EndChild();

			ImGuiX.Separator();

			ImGui.Dummy( new Vector2( windowSize.X - 190, 0 ) );
			ImGui.SameLine();

			ImGui.Button( "Browse" );
			ImGui.SameLine();
			ImGui.Button( "Open" );
			ImGui.SameLine();
			ImGui.Button( "Exit" );

			ImGui.End();
		}
	}
}
