namespace Mocha;

partial class QuakeWalkController
{
	//
	// Movement parameters
	//
	public static float StopSpeed { get; set; } = 0.25f;

	public static float GroundDistance { get; set; } = 0.05f;

	public static float Acceleration { get; set; } = 4.0f;

	public static float AirAcceleration { get; set; } = 4.0f;

	public static float Friction { get; set; } = 6.0f;

	public static float Speed { get; set; } = 8.0f;

	public static float AirSpeed { get; set; } = 4.0f;

	public static float AirControl { get; set; } = 50.0f;

	public static float Gravity { get; set; } = 9.8f;

	public static float MaxWalkAngle { get; set; } = 100f;

	public static float StepSize { get; set; } = 0.5f;

	public static float JumpVelocity { get; set; } = 4.0f;

	public static float Overclip { get; set; } = 1.001f;

	public static float SpeedLimit { get; set; } = 8.0f;
}
