using System;

namespace Minimal;

public class NoClipController : BaseController
{
	public float Friction => 4.0f;
	public float Acceleration => 50.0f;
	public float MaxVelocity => 50.0f;

	private Player Player { get; set; }

	private Vector3 Velocity;

	public NoClipController( Player player )
	{
		Player = player;

		Event.Register( this );
	}

	~NoClipController()
	{
		Event.Unregister( this );
	}

	public override void PredictedUpdate()
	{
		DebugOverlay.ScreenText( $"--------------------------------------------------------------------------------" );
		DebugOverlay.ScreenText( $"{(Core.IsClient ? "Client" : "Server")}" );
		DebugOverlay.ScreenText( $"Velocity: {Velocity}" );

		var wishDir = GetWishDir();

		DebugOverlay.ScreenText( $"wishDir: {wishDir}" );

		Velocity = Move( wishDir, Velocity );

		Move();

		DebugOverlay.ScreenText( $"--------------------------------------------------------------------------------" );
	}

	private Vector3 GetWishDir()
	{
		var eulerRotation = Input.Rotation.ToEulerAngles();
		var rotation = Rotation.From( eulerRotation.WithZ( 0 ) );

		var direction = Input.Direction.WithZ( 0 );

		return (direction * rotation).Normal;
	}

	private Vector3 Accelerate( Vector3 accelDir, Vector3 oldVelocity, float accelerate, float maxSpeed )
	{
		float projVel = Vector3.Dot( oldVelocity, accelDir );
		float accelVel = accelerate * Time.Delta;

		if ( projVel + accelVel > maxSpeed )
			accelVel = maxSpeed - projVel;

		return oldVelocity + accelDir * accelVel;
	}

	private Vector3 Move( Vector3 accelDir, Vector3 oldVelocity )
	{
		float speed = oldVelocity.Length;

		if ( speed != 0 ) // Avoid divide by zero
		{
			float drop = speed * Friction * Time.Delta;
			oldVelocity *= MathF.Max( speed - drop, 0 ) / speed;
		}

		return Accelerate( accelDir, oldVelocity, Acceleration, MaxVelocity );
	}

	public Mocha.TraceResult TraceBBox( Vector3 start, Vector3 end )
	{
		return Cast.Ray( start, end ).WithHalfExtents( Player.PlayerBounds ).Run();//.Ignore( Player ).Run();
	}

	private Vector3 ProjectOnPlane( Vector3 a, Vector3 plane )
	{
		plane = plane.Normal;
		var dot = Vector3.Dot( a, plane );
		return a - plane * dot;
	}

	private void Move()
	{
		var startPos = Player.Position;
		var endPos = startPos + Velocity * Time.Delta;

		Player.Position = endPos;
	}
}
