using System.Reflection;

namespace Mocha.Editor;

partial class Editor
{
	private List<BaseEditorWindow> windows = new();

	private void DrawMenuBar()
	{
		EditorUI.BeginMainMenuBar();

		if ( EditorUI.BeginMenu( $"Tools" ) )
		{
			EditorUI.MenuItem( FontAwesome.Image, "Texture Tool" );
			EditorUI.MenuItem( FontAwesome.FaceGrinStars, "Material Tool" );
			EditorUI.MenuItem( FontAwesome.Cubes, "Model Tool" );
			EditorUI.MenuItem( FontAwesome.Glasses, "Shader Tool" );
			EditorUI.EndMenu();
		}

		foreach ( var window in windows )
		{
			var displayInfo = DisplayInfo.For( window );

			if ( EditorUI.BeginMenu( displayInfo.Category ) )
			{
				var enabled = window.isVisible;
				bool active = EditorUI.MenuItem( displayInfo.TextIcon, displayInfo.Name );

				if ( active )
					window.isVisible = !window.isVisible;

				EditorUI.EndMenu();
			}
		}

		//
		// Buttons
		//
		//{
		//	ImGui.PushStyleVar( ImGuiStyleVar.FramePadding, new System.Numerics.Vector2( 4, 0 ) );
		//	ImGui.PushStyleColor( ImGuiCol.Button, System.Numerics.Vector4.Zero );

		//	// Draw play, pause in center
		//	var center = ImGui.GetMainViewport().WorkSize.X / 2.0f;
		//	center -= 40f; // Approx.
		//	ImGui.SetCursorPosX( center );
		//	ImGui.SetCursorPosY( 8 );

		//	void DrawButtonUnderline()
		//	{
		//		var drawList = ImGui.GetWindowDrawList();
		//		var buttonCol = ImGui.GetColorU32( Colors.Blue );

		//		var p0 = ImGui.GetCursorPos() + new System.Numerics.Vector2( 0, 32 );
		//		var p1 = p0 + new System.Numerics.Vector2( 32, 4 );
		//		drawList.AddRectFilled( p0, p1, buttonCol, 4f );
		//	}

		//	//
		//	// Play button
		//	//
		//	{
		//		if ( World.Current.State == World.States.Playing )
		//		{
		//			DrawButtonUnderline();
		//		}

		//		if ( ImGui.Button( FontAwesome.Play, new System.Numerics.Vector2( 0, 32 ) ) )
		//			World.Current.State = World.States.Playing;
		//	}

		//	//
		//	// Pause button
		//	//
		//	{
		//		if ( World.Current.State == World.States.Paused )
		//		{
		//			DrawButtonUnderline();
		//		}

		//		if ( ImGui.Button( FontAwesome.Pause, new System.Numerics.Vector2( 0, 32 ) ) )
		//			World.Current.State = World.States.Paused;
		//	}

		//	//
		//	// Restart button
		//	//
		//	{
		//		if ( ImGui.Button( FontAwesome.Rotate, new System.Numerics.Vector2( 0, 32 ) ) )
		//			World.Current.ResetWorld();
		//	}

		//	// Draw on right
		//	var right = ImGui.GetMainViewport().WorkSize.X;
		//	right -= 42f;
		//	ImGui.SetCursorPosX( right );
		//	ImGui.SetCursorPosY( 8 );

		//	if ( ImGui.Button( FontAwesome.MagnifyingGlass, new System.Numerics.Vector2( 0, 32 ) ) )
		//	{
		//		quickSwitcherVisible = !quickSwitcherVisible;
		//		quickSwitcherInput = "";
		//	}

		//	ImGui.PopStyleVar();
		//	ImGui.PopStyleColor();
		//}

		EditorUI.EndMainMenuBar();
	}

	public Editor()
	{
		windows.AddRange( Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where( x => typeof( BaseEditorWindow ).IsAssignableFrom( x ) )
			.Where( x => x != typeof( BaseEditorWindow ) )
			.Select( x => Activator.CreateInstance( x ) )
			.OfType<BaseEditorWindow>()
		);
	}

	public void Render()
	{
		DrawMenuBar();

		EditorUI.ShowDemoWindow();

		foreach ( var window in windows )
		{
			if ( window.isVisible )
				window.Render();
		}
	}
}

