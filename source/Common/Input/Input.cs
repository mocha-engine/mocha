namespace Mocha.Common;

public static partial class Input
{
	public static bool Left => Glue.Input.IsButtonDown( 1 );
	public static bool Right => Glue.Input.IsButtonDown( 2 );
	public static bool Middle => Glue.Input.IsButtonDown( 3 );

	public static bool Button4 => Glue.Input.IsButtonDown( 4 );
	public static bool Button5 => Glue.Input.IsButtonDown( 5 );

	// TODO: [ConVar.Archive( "mouse_sensitivity", 2.0f, "Player mouse look sensitivity" )]
	public static float MouseSensitivity { get; set; } = 2.5f;

	public static Vector2 MousePosition => Glue.Input.GetMousePosition();
	public static Vector2 MouseDelta => Glue.Input.GetMouseDelta();

	public static Rotation Rotation { get; private set; } = Rotation.Identity;

	private static float DegreesPerPixel = 0.1f;

	public static void Update()
	{
		var euler = Rotation.ToEulerAngles();
		euler.X += MouseDelta.Y * MouseSensitivity * DegreesPerPixel; // Pitch
		euler.Y += MouseDelta.X * MouseSensitivity * DegreesPerPixel; // Yaw
		Rotation = Rotation.From( euler.X.Clamp( -89, 89 ), euler.Y, 0 );
	}
}
