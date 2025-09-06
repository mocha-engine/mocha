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

		if ( ImGuiX.BeginOverlay( "ConsoleOverlay" ) )
		{
			Vector2 workPos = ImGui.GetMainViewport().WorkPos;
			Vector2 workSize = ImGui.GetMainViewport().WorkSize;
			Vector2 windowSize = ImGui.GetWindowSize();

			ImGui.SetWindowPos( new Vector2( workPos.X + workSize.X - windowSize.X - 16, workPos.Y + workSize.Y - windowSize.Y - 16 ) );

			var logEntries = Log.GetHistory().TakeLast( Count ).ToArray();

			for ( int i = 0; i < logEntries.Length; ++i )
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

		ImGui.End();
	}
}
