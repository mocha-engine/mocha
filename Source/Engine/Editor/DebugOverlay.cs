using Mocha.Editor;

namespace Mocha;

public static class DebugOverlay
{
	struct DebugOverlayText
	{
		public Vector2 position;
		public string text;
		public float time;

		public DebugOverlayText( Vector2 position, string text, float? time = null )
		{
			this.position = position;
			this.text = text;
			this.time = time ?? Time.Delta * 2;
		}
	}

	/// <summary>
	/// This allows us to buffer debug overlay commands in advance.
	/// </summary>
	private readonly static List<DebugOverlayText> screenTextList = new();
	private static int currentLine = 0;

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

	public static void Render()
	{
		for ( int i = 0; i < screenTextList.Count; i++ )
		{
			var item = screenTextList[i];
			InternalScreenText( item.position, item.text );

			if ( item.time > 0 )
			{
				item.time -= Time.Delta;
				screenTextList[i] = item;

				if ( item.time <= 0 )
					screenTextList.Remove( item );
			}
		}

		currentLine = 0;
	}

	public static void ScreenText( int line, object obj )
	{
		line++;

		var lineHeight = ImGui.GetTextLineHeightWithSpacing();
		screenTextList.Add( new( new Vector2( 32, lineHeight * line ), obj.ToString()! ) );
	}

	public static void ScreenText( object obj )
	{
		currentLine++;

		var lineHeight = ImGui.GetTextLineHeightWithSpacing();
		screenTextList.Add( new( new Vector2( 32, lineHeight * currentLine ), obj.ToString()! ) );
	}
}
