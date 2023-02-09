global using ImGuiNET;

namespace Mocha.Editor;

public class Editor
{
	static bool drawPerformanceOverlay = false;

	public static List<EditorWindow> EditorWindows = new()
	{
		new ConsoleWindow(),
		new BrowserWindow(),
		new InspectorWindow()
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

			if ( ImGui.BeginMenu( "Layout" ) )
			{
				if ( ImGui.MenuItem( "Reset to Default" ) )
					ResetLayout();

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
		if ( ImGuiX.BeginOverlay( "PerformanceOverlay" ) )
		{
			Vector2 workPos = ImGui.GetMainViewport().WorkPos;
			Vector2 workSize = ImGui.GetMainViewport().WorkSize;
			Vector2 windowSize = ImGui.GetWindowSize();

			ImGui.SetWindowPos( new Vector2( workPos.X + workSize.X - windowSize.X - 16, workPos.Y + workSize.Y - windowSize.Y - 128 - 16 ) );

			var cursorPos = ImGui.GetCursorPos();
			void DrawProperty( string name, string value )
			{
				ImGuiX.TextBold( name );
				ImGui.SameLine();

				var textWidth = ImGui.CalcTextSize( value ).X;
				ImGui.SetCursorPosX( cursorPos.X + 128 - textWidth );
				ImGui.Text( value );
			}

			{
				var left = $"{Time.FPS} FPS";
				var right = $"{Time.Delta * 1000f:F0}ms";

				ImGuiX.TextSubheading( left );
				ImGui.SameLine();

				var textWidth = ImGui.CalcTextSize( right ).X * 1.15f;
				ImGui.SetCursorPosX( cursorPos.X + 128 - textWidth );
				ImGui.SetCursorPosY( cursorPos.Y );
				ImGuiX.TextSubheading( right );
			}

			var fpsHistory = Time.FPSHistory.Select( x => (float)x ).ToArray();
			var scaleMax = fpsHistory.Max();

			ImGui.PushStyleColor( ImGuiCol.FrameBg, Vector4.Zero );
			ImGui.PushStyleVar( ImGuiStyleVar.FramePadding, new Vector2( 0, 0 ) );
			ImGui.PlotHistogram( "##FrameTimes", ref fpsHistory[0], Time.FPSHistory.Count, 0, "", 0f, scaleMax, new Vector2( 128f, 32 ) );
			ImGui.PopStyleVar();
			ImGui.PopStyleColor();

			ImGuiX.Separator( new Vector4( 1, 1, 1, 0.05f ) );

			var min = fpsHistory.Min();
			DrawProperty( $"Min", $"{min:F0}fps" );
			var max = fpsHistory.Max();
			DrawProperty( $"Max", $"{max:F0}fps" );
			var avg = fpsHistory.Average();
			DrawProperty( $"Avg", $"{avg:F0}fps" );

			ImGuiX.Separator( new Vector4( 1, 1, 1, 0.05f ) );

			DrawProperty( $"Elapsed time", $"{Time.Now:F0}s" );
			DrawProperty( $"Current tick", $"{NativeEngine.GetCurrentTick():F0}" );
			DrawProperty( $"Tick rate", $"{Core.TickRate}" );

			ImGuiX.Separator( new Vector4( 1, 1, 1, 0.05f ) );

			DrawProperty( $"Ping", $"{0}ms" );
			DrawProperty( $"Jitter", $"{0}ms" );
			DrawProperty( $"Loss", $"{0}" );
		}

		ImGui.End();
	}

	private static void ResetLayout()
	{

	}
}
