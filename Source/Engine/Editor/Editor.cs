global using ImGuiNET;

namespace Mocha.Editor;

public class Editor
{
	static bool drawPerformanceOverlay = false;

	public static List<EditorWindow> EditorWindows = new()
	{
		// new MaterialEditorWindow(),
		new ConsoleWindow(),
		new BrowserWindow()
		// new MemoryWindow()
	};

	public static void Draw()
	{
		DrawMenuBar();
		DrawStatusBar();

		if ( drawPerformanceOverlay )
			DrawPerformanceOverlay();

		foreach ( var window in EditorWindows.ToArray() )
		{
			if ( window.isVisible )
				window.Draw();
		}
	}

	private static void DrawMenuBar()
	{
		if ( ImGui.BeginMainMenuBar() )
		{
			ImGui.Dummy( new Vector2( 4, 0 ) );
			ImGui.Text( "Mocha Engine" );
			ImGui.Dummy( new Vector2( 4, 0 ) );

			ImGui.Separator();
			ImGui.Dummy( new Vector2( 4, 0 ) );

			if ( ImGui.BeginMenu( "Window" ) )
			{
				foreach ( var window in EditorWindows )
				{
					var displayInfo = DisplayInfo.For( window );
					if ( ImGui.MenuItem( displayInfo.Name ) )
						window.isVisible = !window.isVisible;
				}

				if ( ImGui.MenuItem( "Performance Overlay" ) )
					drawPerformanceOverlay = !drawPerformanceOverlay;

				ImGui.EndMenu();
			}

			ImGuiX.RenderViewDropdown();
		}

		ImGui.EndMainMenuBar();
	}

	private static void DrawStatusBar()
	{
		if ( ImGuiX.BeginMainStatusBar() )
		{
			ImGui.Text( $"{Screen.Size.X}x{Screen.Size.Y}" );

			ImGui.Dummy( new Vector2( 4, 0 ) );
			ImGui.Separator();
			ImGui.Dummy( new Vector2( 4, 0 ) );
			ImGui.Text( $"{Time.FPS} FPS" );

			// Filler
			var windowWidth = ImGui.GetWindowWidth();
			var cursorX = ImGui.GetCursorPosX();
			ImGui.Dummy( new Vector2( windowWidth - cursorX - 150f, 0 ) );

			ImGui.Separator();
			ImGui.Dummy( new Vector2( 4, 0 ) );
			ImGui.Text( "Press ~ to toggle cursor" );
		}

		ImGuiX.EndMainStatusBar();
	}

	private static void DrawPerformanceOverlay()
	{
		if ( ImGuiX.BeginOverlay( "Time" ) )
		{
			var gpuName = ImGuiX.GetGPUName();

			ImGui.Text( $"GPU: {gpuName}" );
			ImGui.Text( $"FPS: {Time.FPS}" );
			ImGui.Text( $"Current time: {Time.Now}" );
			ImGui.Text( $"Frame time: {(Time.Delta * 1000f).CeilToInt()}ms" );

			ImGui.End();
		}
	}
}
