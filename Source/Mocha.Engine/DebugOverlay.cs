namespace Mocha;

public struct DebugOverlayText
{
	public Vector2 position;
	public string text;
	public float time;

	public DebugOverlayText( Vector2 position, string text, float? time = null )
	{
		this.position = position;
		this.text = text;
		this.time = time ?? 0f;
	}
}

/// <summary>
/// Public API for <see cref="Mocha.Editor.DebugOverlay"/>
/// </summary>
public static partial class DebugOverlay
{
	/// <summary>
	/// This allows us to buffer debug overlay commands in advance.
	/// </summary>
	public readonly static List<DebugOverlayText> screenTextList = new();
	public static int currentLine = 0;

	public static void ScreenText( int line, object obj )
	{
		line++;

		var lineHeight = 16.0f;
		screenTextList.Add( new( new Vector2( 32, lineHeight * line ), obj.ToString()! ) );
	}

	public static void ScreenText( object obj )
	{
		currentLine++;

		var lineHeight = 16.0f;
		screenTextList.Add( new( new Vector2( 32, lineHeight * currentLine ), obj.ToString()! ) );
	}
}
