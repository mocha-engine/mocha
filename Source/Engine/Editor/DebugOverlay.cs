using Mocha.Editor;

namespace Mocha;

public static class DebugOverlay
{
	private static int CurrentLine { get; set; }
	public static void NewFrame()
	{
		CurrentLine = 0;
	}

	private static void InternalScreenText( Vector2 position, object obj )
	{
		//
		// Setup invisible window
		//
		ImGui.SetNextWindowViewport( ImGui.GetMainViewport().ID );
		ImGui.SetNextWindowPos( ImGui.GetMainViewport().WorkPos );
		ImGui.SetNextWindowSize( ImGui.GetMainViewport().WorkSize );

		ImGui.Begin( "debugoverlay", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoTitleBar );
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
		ImGui.End();
	}

	public static void ScreenText( int line, object obj )
	{
		line++;

		var lineHeight = ImGui.GetTextLineHeightWithSpacing();
		InternalScreenText( new Vector2( 32, lineHeight * line ), obj );
	}

	public static void ScreenText( object obj )
	{
		CurrentLine++;

		var lineHeight = ImGui.GetTextLineHeightWithSpacing();
		InternalScreenText( new Vector2( 32, lineHeight * CurrentLine ), obj );
	}
}
