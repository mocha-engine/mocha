namespace Mocha.Common;

public static partial class Input
{
	public static bool Left => Glue.Input.IsButtonDown( 1 );
	public static bool Middle => Glue.Input.IsButtonDown( 2 );
	public static bool Right => Glue.Input.IsButtonDown( 3 );

	public static bool Button4 => Glue.Input.IsButtonDown( 4 );
	public static bool Button5 => Glue.Input.IsButtonDown( 5 );

	// TODO: [ConVar.Archive( "mouse_sensitivity", 2.0f, "Player mouse look sensitivity" )]
	public static float MouseSensitivity { get; set; } = 2.5f;

	public static Vector2 MousePosition => Glue.Input.GetMousePosition();
	public static Vector2 MouseDelta => Glue.Input.GetMouseDelta();

	public static Rotation Rotation { get; private set; } = Rotation.Identity;

	private static float DegreesPerPixel = 0.1f;

	public static Vector3 Direction { get; private set; }

	private static bool IsKeyDown( InputButton key ) => Glue.Input.IsKeyDown( (int)key );

	public static bool Jump => IsKeyDown( InputButton.KeySpace );
	public static bool Crouch => IsKeyDown( InputButton.KeyControl );
	public static bool Sprint => IsKeyDown( InputButton.KeyShift );

	public static void Update()
	{
		//
		// Rotation
		//
		var euler = Rotation.ToEulerAngles();
		euler.X += MouseDelta.Y * MouseSensitivity * DegreesPerPixel; // Pitch
		euler.Y += MouseDelta.X * MouseSensitivity * DegreesPerPixel; // Yaw
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
