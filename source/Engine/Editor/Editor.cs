global using ImGui = Mocha.Glue.Editor;

namespace Mocha.Editor;

public class Editor
{
	static bool drawPerformanceOverlay = false;

	public static List<EditorWindow> EditorWindows = new()
	{
		new MaterialEditorWindow(),
		new ConsoleWindow(),
		new MemoryWindow()
	};

	public static void Draw()
	{
		DrawMenuBar();

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

			ImGui.RenderViewDropdown();
		}

		ImGui.EndMainMenuBar();
	}

	private static void DrawPerformanceOverlay()
	{
		if ( ImGui.BeginOverlay( "Time" ) )
		{
			var gpuName = ImGui.GetGPUName();

			ImGui.Text( $"GPU: {gpuName}" );

			float fps = 1.0f / Time.Delta;
			ImGui.Text( $"FPS: {fps.CeilToInt()}" );

			ImGui.Text( $"Current time: {Time.Now}" );
			ImGui.Text( $"Frame time: {(Time.Delta * 1000f).CeilToInt()}ms" );
		}

		ImGui.End();
	}
}
