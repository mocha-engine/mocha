﻿namespace Minimal;

public class Player : Mocha.Player
{
	public BaseController WalkController { get; private set; }

	public float Health { get; set; }

	protected override void Spawn()
	{
		// TODO: This would be better as just a ctor
		base.Spawn();

		PlayerBounds = new( 0.5f, 0.5f, 1.8f ); // Metres
		// SetCubePhysics( PlayerBounds, false );
	}

	private void UpdateEyeTransform()
	{
		EyePosition = Position + Vector3.Up * PlayerBounds.Z;
		EyeRotation = Input.Rotation;
	}

	public override void Respawn()
	{
		base.Respawn();

		WalkController = new NoClipController( this );
		// Velocity = Vector3.Zero;
		Position = new Vector3( 0.0f, 4.0f, 5.0f );
	}

	[Event.Tick]
	public void PredictedUpdate()
	{
		UpdateEyeTransform();
		WalkController.PredictedUpdate();
	}

	bool wasCrouch = false;

	public override void FrameUpdate()
	{
		if ( Input.Crouch && !wasCrouch )
		{
			if ( WalkController is WalkController )
				WalkController = new NoClipController( this );
			else
				WalkController = new WalkController( this );
		}

		wasCrouch = Input.Crouch;

		UpdateCamera();
		UpdateEyeTransform();

		Health = MathX.Sin01( Time.Now ) * 100f;
	}

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

		//
		// Field of view
		//
		Camera.FieldOfView = 90f;

		//
		// Z planes
		//
		Camera.ZNear = 0.01f;
		Camera.ZFar = 1000.0f;
	}
}
