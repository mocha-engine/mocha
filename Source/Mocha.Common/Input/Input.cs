namespace Mocha.Common;

public static partial class Input
{
	private static Glue.InputManager NativeInput => Engine.GetInputManager();

	public static bool Left => NativeInput.IsButtonDown( 1 );
	public static bool Middle => NativeInput.IsButtonDown( 2 );
	public static bool Right => NativeInput.IsButtonDown( 3 );

	public static bool Button4 => NativeInput.IsButtonDown( 4 );
	public static bool Button5 => NativeInput.IsButtonDown( 5 );

	// TODO: [ConVar.Archive( "mouse_sensitivity", 2.0f, "Player mouse look sensitivity" )]
	public static float MouseSensitivity { get; set; } = 2.5f;

	public static Vector2 MousePosition => NativeInput.GetMousePosition();
	public static Vector2 MouseDelta => NativeInput.GetMouseDelta();

	public static Rotation Rotation { get; private set; } = Rotation.Identity;

	public static Vector3 Direction { get; private set; }

	private static bool IsKeyDown( InputButton key ) => NativeInput.IsKeyDown( (int)key );

	public static bool Jump => IsKeyDown( InputButton.KeySpace );
	public static bool Crouch => IsKeyDown( InputButton.KeyControl );
	public static bool Sprint => IsKeyDown( InputButton.KeyShift );

	public static void Update()
	{
		//
		// Rotation
		//
		var euler = Rotation.ToEulerAngles();
		euler.X += MouseDelta.Y * MouseSensitivity * Time.Delta; // Pitch
		euler.Y += MouseDelta.X * MouseSensitivity * Time.Delta; // Yaw
		Rotation = Rotation.From( euler.X.Clamp( -89, 89 ), euler.Y, 0 );

		//
		// Forward
		//
		float forward = 0.0f;
		if ( IsKeyDown( InputButton.KeyW ) )
			forward += 1.0f;

		if ( IsKeyDown( InputButton.KeyS ) )
			forward -= 1.0f;

		//
		// Right
		//
		float right = 0.0f;
		if ( IsKeyDown( InputButton.KeyD ) )
			right += 1.0f;

		if ( IsKeyDown( InputButton.KeyA ) )
			right -= 1.0f;

		//
		// Up
		//
		float up = 0.0f;
		if ( IsKeyDown( InputButton.KeySpace ) )
			up += 1.0f;

		if ( IsKeyDown( InputButton.KeyControl ) )
			up -= 1.0f;

		//
		// Combine, store in Direction
		//
		Direction = new Vector3( forward, right, up );
	}
}
