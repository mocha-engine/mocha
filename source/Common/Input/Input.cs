namespace Mocha.Common;

public static partial class Input
{
	public enum MouseModes
	{
		Locked,
		Unlocked
	}

	public static MouseModes MouseMode { get; set; }

	public static Vector2 MouseDelta { get; set; }
	public static Vector2 MousePosition { get; set; }

	public static float Forward { get; set; }
	public static float Left { get; set; }
	public static float Up { get; set; }

	public static bool MouseLeft { get; set; }
	public static bool MouseRight { get; set; }

	public static List<InputButton> KeysDown { get; set; } = new();
	public static List<InputButton> LastKeysDown { get; set; } = new();

	public static unsafe void Update()
	{
	}

	private static bool IsKeyPressed( InputButton b ) => KeysDown.Contains( b );
	private static bool WasKeyPressed( InputButton b ) => LastKeysDown.Contains( b );

	public static bool Pressed( InputButton button )
	{
		return IsKeyPressed( button )
			&& !WasKeyPressed( button );
	}

	public static bool Down( InputButton button )
	{
		return IsKeyPressed( button );
	}

	public static bool Released( InputButton button )
	{
		return !IsKeyPressed( button )
			&& WasKeyPressed( button );
	}
}
