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
			"MyCoolGame",
			"ABC",
			"DEF",
			"GHI"
		};
	}

	private void DrawProject( string project )
	{
		bool selected = false;

		float w = ImGui.GetWindowSize().X - 100;

		if ( ImGui.GetCursorPos().X > w )
		{
			ImGui.NewLine();
			ImGuiX.BumpCursorX( 8 );
		}

		ImGuiX.Icon( project, "textures/placeholder.mtex", Vector4.One, ref selected );

		ImGui.SameLine();
	}

	public override void Draw()
	{
		if ( ImGuiX.BeginWindow( "Project Browser", ref isVisible ) )
		{
			ImGuiX.Title( $"{FontAwesome.Folder} Your Projects", "Here's a list of your projects. Click one to load it, or click 'New' to make a new one.", drawSubpanel: true );

			ImGuiX.Separator();

			if ( ImGui.BeginChild( "##projects_list", new Vector2( -1, -52 ), false, ImGuiWindowFlags.AlwaysUseWindowPadding ) )
			{
				float padding = 8f;
				ImGuiX.BumpCursorX( padding );
				ImGuiX.BumpCursorY( padding );

				foreach ( var project in _projects )
				{
					DrawProject( project );
				}
			}

			ImGui.EndChild();

			ImGuiX.Separator();

			using ( var layout = new HorizontalLayout( "new_project", LayoutAlignment.Start ) )
			{
				layout.Add( ImGui.Button( "New..." ) );
			}

			ImGui.SameLine();

			using ( var layout = new HorizontalLayout( "browse_and_open", LayoutAlignment.End ) )
			{
				layout.Add( ImGui.Button( "Browse" ) );
				layout.Add( ImGui.Button( "Open" ) );

				if ( layout.Add( ImGui.Button( "Exit" ) ) )
				{
					Environment.Exit( 0 ); // TODO: Graceful exit
				}
			}

			ImGui.End();
		}
	}
}
