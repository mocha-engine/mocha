namespace Mocha.Editor;

public static partial class DebugOverlay
{
	private static void InternalScreenText( Vector2 position, object obj )
	{
		//
		// Setup invisible window
		//
		ImGui.SetNextWindowViewport( ImGui.GetMainViewport().ID );
		ImGui.SetNextWindowPos( ImGui.GetMainViewport().WorkPos );
		ImGui.SetNextWindowSize( ImGui.GetMainViewport().WorkSize );

		if ( ImGui.Begin( "debugoverlay", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoTitleBar ) )
		{
			position += (Vector2)ImGui.GetWindowPos();

			//
			// Draw shadow first (under main label)
			//
			ImGui.SetCursorScreenPos( position + new Vector2( 1, 1 ) );
			ImGui.PushStyleColor( ImGuiCol.Text, new Vector4( 0, 0, 0, 1 ) );
			ImGuiX.TextMonospace( obj?.ToString() ?? "null" );
			ImGui.PopStyleColor();

			//
			// Draw main label
			//
			ImGui.SetCursorScreenPos( position );
			ImGuiX.TextMonospace( obj?.ToString() ?? "null" );

			//
			// Cleanup
			//
			ImGui.SetCursorScreenPos( System.Numerics.Vector2.Zero );

			//
			// ImGui: Submit an item to validate extent
			// https://github.com/ocornut/imgui/releases/tag/v1.89#:~:text=Instead%2C%20please-,submit%20an%20item,-%3A%0ABegin(...)
			//
			ImGui.Dummy( new Vector2( 0, 0 ) );
			ImGui.End();
		}
	}

	public static void Render()
	{
		var screenTextList = Mocha.DebugOverlay.screenTextList;

		for ( int i = 0; i < screenTextList.Count; i++ )
		{
			var item = screenTextList[i];
			InternalScreenText( item.position, item.text );

			if ( item.time > 0 )
			{
				item.time -= Time.Delta;
				screenTextList[i] = item;
			}
		}

		Mocha.DebugOverlay.currentLine = 0;
	}

	public static void Clear()
	{
		var screenTextList = Mocha.DebugOverlay.screenTextList;

		for ( int i = 0; i < screenTextList.Count; i++ )
		{
			var item = screenTextList[i];

			if ( item.time <= 0 )
				screenTextList.Remove( item );
		}
	}
}
