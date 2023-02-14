using System;

namespace Minimal;

/*
 * This is just a test for now. Will get put in base engine
 * once it's done
 */

public class WalkController
{
	public float Friction => 6.0f;
	public float GroundAccelerate => 20.0f;
	public float MaxVelocityGround => 20.0f;
	public float AirAccelerate => 1.0f;
	public float MaxVelocityAir => 20.0f;
	public float GroundDistance => 1.0f;

	private bool IsGrounded => GroundEntity != null;
	private BaseEntity GroundEntity;

	private Player Player { get; set; }

	private Vector3 Velocity;

	public WalkController( Player player )
	{
		Player = player;
		Player.IgnoreRigidbodyRotation = true;

		Event.Register( this );
	}

	[Event.Tick]
	public void PredictedUpdate()
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

		Player.Velocity = Velocity;
		Player.Position = Player.Position + Velocity * Time.Delta;

		CategorizePosition();

		DebugOverlay.ScreenText( $"--------------------------------------------------------------------------------" );
	}

	private Vector3 GetWishDir()
	{
		var eulerRotation = Input.Rotation.ToEulerAngles();
		var rotation = Rotation.From( eulerRotation.WithX( 0 ).WithZ( 0 ) );

		var direction = Input.Direction.WithZ( 0 );

		return direction * rotation;
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
		return Cast.Ray( start, end ).WithHalfExtents( Player.PlayerHalfExtents ).Ignore( Player ).Run();
	}

	private void CheckGrounded()
	{
		var tr = TraceBBox( Player.Position, Player.Position + Vector3.Down * GroundDistance );

		GroundEntity = tr.Entity;
	}

	private void CategorizePosition()
	{
		if ( !IsGrounded )
			return;

		var tr = TraceBBox( Player.Position, Player.Position + Vector3.Down * GroundDistance );

		Player.Position = tr.EndPosition + Vector3.Up * GroundDistance;
	}
}
