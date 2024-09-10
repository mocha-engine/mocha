using System;

namespace Minimal;

/*
 * This is just a test for now. Will get put in base engine
 * once it's done
 */

public class WalkController : BaseController
{
	public float Friction => 12.0f;
	public float GroundAccelerate => 50.0f;
	public float MaxVelocityGround => 50.0f;
	public float AirAccelerate => 5.0f;
	public float MaxVelocityAir => 100.0f;
	public float GroundDistance => 0.1f;
	public float StepSize => 0.5f;
	public float MaxAngle => 60.0f;

	private bool IsGrounded => GroundEntity != null;
	private Actor GroundEntity;

	private Player Player { get; set; }

	private Vector3 Velocity;

	public WalkController( Player player )
	{
		Player = player;
		// Player.IgnoreRigidbodyRotation = true;

		Event.Register( this );
	}

	public override void PredictedUpdate()
	{
		DebugOverlay.ScreenText( $"--------------------------------------------------------------------------------" );
		DebugOverlay.ScreenText( $"{(Core.IsClient ? "Client" : "Server")}" );
		DebugOverlay.ScreenText( $"Velocity: {Velocity}" );
		DebugOverlay.ScreenText( $"GroundEntity: {GroundEntity?.Name ?? "None"}" );
		DebugOverlay.ScreenText( $"IsGrounded: {IsGrounded}" );

		CheckGrounded();
		var wishDir = GetWishDir();

		DebugOverlay.ScreenText( $"wishDir: {wishDir}" );
		DebugOverlay.ScreenText( $"GroundAccelerate: {GroundAccelerate}" );

		if ( IsGrounded )
		{
			Velocity = MoveGround( wishDir, Velocity );

			Velocity.Z = 0;

			if ( Input.Jump )
			{
				Velocity.Z += 4.0f;
				GroundEntity = null;
			}
		}
		else
		{
			Velocity = MoveAir( wishDir, Velocity );

			Velocity.Z -= 9.8f * Time.Delta;
		}

//		Player.Velocity = Velocity * 10f;
		Move();

		DebugOverlay.ScreenText( $"--------------------------------------------------------------------------------" );
	}

	~WalkController()
	{
		Event.Unregister( this );
	}

	private Vector3 GetWishDir()
	{
		var eulerRotation = Input.Rotation.ToEulerAngles();
		var rotation = Rotation.From( eulerRotation.WithX( 0 ).WithZ( 0 ) );

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

	private Vector3 MoveGround( Vector3 accelDir, Vector3 oldVelocity )
	{
		float speed = oldVelocity.Length;

		if ( speed != 0 ) // Avoid divide by zero
		{
			float drop = speed * Friction * Time.Delta;
			oldVelocity *= MathF.Max( speed - drop, 0 ) / speed;
		}

		return Accelerate( accelDir, oldVelocity, GroundAccelerate, MaxVelocityGround );
	}

	private Vector3 MoveAir( Vector3 accelDir, Vector3 prevVelocity )
	{
		return Accelerate( accelDir, prevVelocity, AirAccelerate, MaxVelocityAir );
	}

	public Mocha.TraceResult TraceBBox( Vector3 start, Vector3 end )
	{
		return Cast.Ray( start, end ).WithHalfExtents( Player.PlayerBounds ).Run();//.Ignore( Player ).Run();
	}

	private void CheckGrounded()
	{
		var tr = TraceBBox( Player.Position, Player.Position + Vector3.Down * GroundDistance );

		// Grounded only counts if the normal is facing upwards
		var angle = Vector3.GetAngle( tr.Normal, Vector3.Up );

		//if ( tr.Hit && angle < MaxAngle )
		//	GroundEntity = tr.Entity;
		//else
			GroundEntity = null;
	}

	private Vector3 ProjectOnPlane( Vector3 a, Vector3 plane )
	{
		plane = plane.Normal;
		var dot = Vector3.Dot( a, plane );
		return a - plane * dot;
	}

	private (bool wasSuccess, Vector3 endPosition) SlideMove( Vector3 startPos, Vector3 endPos )
	{
		var tr = TraceBBox( startPos, endPos );

		if ( tr.Fraction < 1 )
		{
			// The player is colliding with a wall
			var normal = tr.Normal;
			var newVel = ProjectOnPlane( Velocity, normal );
			endPos = startPos + newVel * Time.Delta;

			// Check for collisions with adjacent walls
			var tr2 = TraceBBox( endPos, endPos + Vector3.Up * StepSize );
			if ( tr2.Fraction == 0 )
			{
				// The player is stuck in a corner
				var sideVel = ProjectOnPlane( Velocity, tr2.Normal );
				newVel += sideVel;
				endPos = startPos + newVel * Time.Delta;
			}
		}

		var finalTr = TraceBBox( startPos, endPos );
		Player.Position = finalTr.EndPosition;

		return (true, finalTr.EndPosition);
	}

	private MoveResult StepMove( Vector3 startPos, Vector3 endPos )
	{
		var tr = TraceBBox( startPos, endPos );

		if ( tr.Fraction < 1 )
		{
			// Try stepping up
			var stepPos = endPos + Vector3.Up * StepSize;
			var stepTr = TraceBBox( stepPos, stepPos );

			if ( stepTr.Fraction > 0 )
			{
				// Trace back down to see how far we should step
				var stepDownTr = TraceBBox( stepPos, stepPos + Vector3.Down * StepSize );
				return (true, stepDownTr.EndPosition);
			}
		}

		return (false, endPos);
	}

	private void Move()
	{
		var startPos = Player.Position;
		var endPos = startPos + Velocity * Time.Delta;

		var (stepSuccess, stepPosition) = StepMove( startPos, endPos );
		if ( stepSuccess )
		{
			Player.Position = stepPosition;
			return;
		}

		var (slideSuccess, slidePosition) = SlideMove( startPos, stepPosition );
		if ( slideSuccess )
		{
			Player.Position = slidePosition;
			return;
		}
	}
}

internal record struct MoveResult( bool wasSuccess, Vector3 endPosition )
{
	public static implicit operator (bool wasSuccess, Vector3 endPosition)( MoveResult value )
	{
		return (value.wasSuccess, value.endPosition);
	}

	public static implicit operator MoveResult( (bool wasSuccess, Vector3 endPosition) value )
	{
		return new MoveResult( value.wasSuccess, value.endPosition );
	}
}
