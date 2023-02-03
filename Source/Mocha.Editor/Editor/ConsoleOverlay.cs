namespace Mocha.Editor;

public static partial class ConsoleOverlay
{
	private const int Count = 5;

	public static void Render()
	{
		//
		// Setup invisible window
		//
		ImGui.SetNextWindowViewport( ImGui.GetMainViewport().ID );
		ImGui.SetNextWindowSize( new Vector2( 0 ) );

		ImGui.PushStyleColor( ImGuiCol.WindowBg, Theme.Gray.ToBackground( 0.75f ) );
		ImGui.PushStyleColor( ImGuiCol.Border, Theme.Transparent );
		ImGui.PushStyleVar( ImGuiStyleVar.WindowRounding, 0 );

		if ( ImGui.Begin( "consoleoverlay", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoTitleBar ) )
		{
			var logEntries = Log.GetHistory().TakeLast( Count ).ToArray();

			for ( int i = 0; i < Count; ++i )
			{
				var logEntry = logEntries[i];
				var alpha = i / (float)Count;

				alpha = MathX.LerpInverse( 0.75f, 1.0f, alpha );

				var cursorPos = ImGui.GetCursorPos();

				ImGuiX.SetCursorPosRelative( new Vector2( 1, 1 ) );

				ImGui.PushStyleColor( ImGuiCol.Text, new Vector4( 0, 0, 0, alpha ) );
				ImGuiX.TextMonospace( logEntry.message );
				ImGui.PopStyleColor();

				ImGui.SetCursorPos( cursorPos );

				ImGui.PushStyleColor( ImGuiCol.Text, new Vector4( 1, 1, 1, alpha ) );
				ImGuiX.TextMonospace( logEntry.message );
				ImGui.PopStyleColor();
			}
		}

		var windowSize = ImGui.GetWindowSize();
		var windowPos = ImGui.GetMainViewport().WorkPos + new System.Numerics.Vector2( ImGui.GetMainViewport().WorkSize.X - windowSize.X, ImGui.GetMainViewport().WorkSize.Y - windowSize.Y );
		ImGui.SetWindowPos( windowPos );

		ImGui.End();

		ImGui.PopStyleColor( 2 );
		ImGui.PopStyleVar();
	}
}
