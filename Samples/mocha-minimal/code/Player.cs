﻿namespace Minimal;

public class Player : Mocha.Player
{
	private Vector3 PlayerBounds = new( 0.5f, 0.5f, 1.8f ); // Metres

	public QuakeWalkController WalkController { get; private set; }

	[Category( "Player" )]
	public bool IsGrounded => WalkController.IsGrounded;

	[Category( "Player" )]
	public BaseEntity GroundEntity => WalkController.GroundEntity;

	public float Health { get; set; }

	private void UpdateEyeTransform()
	{
		EyePosition = Position + Vector3.Up * PlayerHalfExtents.Z * 2f;
		EyeRotation = Input.Rotation;
	}

	public override void Respawn()
	{
		base.Respawn();

		PlayerHalfExtents = PlayerBounds / 2f;

		WalkController = new( this );
		Velocity = Vector3.Zero;
		Position = new Vector3( 0.0f, 4.0f, 0.9f );
	}

	private TimeSince TimeSinceFire;

	public override void Update()
	{
		UpdateEyeTransform();
		WalkController.Update();

		//
		// Spawn some balls when clicking
		//
		if ( Input.Left && TimeSinceFire > 0.1f )
		{
			var tr = Cast.Ray( EyeRay, 10f ).Ignore( this ).Run();

			var ball = new ModelEntity( "core/models/dev/dev_ball.mmdl" );
			ball.Name = "My Ball";
			ball.Position = tr.EndPosition + tr.Normal * 1.0f;
			ball.Restitution = 1.0f;
			ball.Friction = 1.0f;
			ball.Mass = 10.0f;

			ball.SetSpherePhysics( 0.5f, false );

			TimeSinceFire = 0;
		}
	}

	public override void FrameUpdate()
	{
		UpdateCamera();
		UpdateEyeTransform();

		Health = MathX.Sin01( Time.Now ) * 100f;
	}

	float lastHeight = 1.8f;
	float lastFov = 90f;

	private void UpdateCamera()
	{
		//
		// Rotation
		//
		Camera.Rotation = EyeRotation;

		//
		// Position
		//
		Camera.Position = Position + LocalEyePosition;

		// Smooth out z-axis so that stairs, crouching are not sudden changes
		Camera.Position = Camera.Position.WithZ( lastHeight.LerpTo( Camera.Position.Z, 10f * Time.Delta ) );
		lastHeight = Camera.Position.Z;

		//
		// Field of view
		//
		float targetFov = 90f;

		// Interpolate velocity when sprinting
		if ( WalkController?.Sprinting ?? false && Velocity.WithZ( 0 ).Length > 1.0f )
			targetFov = 100f;

		Camera.FieldOfView = lastFov.LerpTo( targetFov, 10 * Time.Delta );
		lastFov = Camera.FieldOfView;

		//
		// Z planes
		//
		Camera.ZNear = 0.01f;
		Camera.ZFar = 1000.0f;
	}
}