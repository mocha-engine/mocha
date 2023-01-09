namespace Mocha;

//
// This isn't exactly like the Quake movement controller
// ( some quirks have been tweaked or removed ) but it's much
// closer to arena shooter movement than the default WalkController
//
//
// https://github.com/id-Software/Quake-III-Arena/blob/master/code/game/bg_pmove.c
//
public partial class QuakeWalkController
{
	public bool Crouching { get; private set; }
	public bool Sprinting { get; private set; }
	private bool Walking { get; set; }
	private bool GroundPlane { get; set; }

	public Player Player { get; set; }

	public Vector3 Velocity
	{
		get => Player.Velocity;
		set => Player.Velocity = value;
	}

	public Vector3 Position
	{
		get => Player.Position;
		set => Player.Position = value;
	}

	public TraceResult GroundTrace { get; set; }
	public BaseEntity GroundEntity { get; set; }
	public Vector3 GroundNormal { get; set; }
	public bool IsGrounded => GroundEntity != null;

	public QuakeWalkController( Player player )
	{
		Player = player;
	}

	public void Update()
	{
		if ( SpeedLimit > 0f )
		{
			if ( Velocity.WithZ( 0 ).Length > SpeedLimit )
			{
				Velocity = (Velocity.Normal * SpeedLimit).WithZ( Velocity.Z );
			}
		}

		// update duck
		CheckCrouch();

		// update sprint
		CheckSprint();

		// set player eye height
		UpdateEyePosition();

		// set groundentity
		TraceToGround();

		// movement
		if ( CheckJump() || !IsGrounded )
		{
			// gravity start
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

			// jumped away or in air
			AirMove();
		}
		else
		{
			// walking on ground
			WalkMove();
		}

		// stick to ground
		CategorizePosition();

		// set groundentity
		TraceToGround();
	}

	private void CheckSprint()
	{
		Sprinting = !Crouching && Input.Sprint;
	}

	private void CheckCrouch()
	{
		Crouching = Input.Crouch;
	}

	private void UpdateEyePosition()
	{
		if ( Crouching )
			Player.LocalEyePosition = Vector3.Up * 1.8f * 0.15f;
		else
			Player.LocalEyePosition = Vector3.Up * 1.8f * 0.5f;
	}

	private Vector3 ClipVelocity( Vector3 inVec, Vector3 normal, float overbounce )
	{
		float backoff = inVec.Dot( normal );

		if ( backoff < 0 )
			backoff *= overbounce;
		else
			backoff /= overbounce;

		Vector3 outVec = inVec - (normal * backoff);
		return outVec;
	}

	private void ApplyFriction()
	{
		if ( !IsGrounded )
			return;

		Vector3 vec = Velocity;

		if ( Walking )
			vec.Z = 0;

		float speed = vec.Length;
		float control;

		if ( speed < 0.01f )
		{
			Vector3 vel = new( Velocity );
			vel.X = 0;
			vel.Y = 0;
			Velocity = vel;
			return;
		}

		float drop = 0;

		if ( Walking )
		{
			control = speed < StopSpeed ? StopSpeed : speed;
			drop += control * Friction * Time.Delta;
		}

		float newspeed = speed - drop;

		if ( newspeed < 0 )
			newspeed = 0;

		newspeed /= speed;

		Velocity *= newspeed;
	}

	private void Accelerate( Vector3 wishDir, float wishSpeed, float maxSpeed, float accel )
	{
		float currentspeed = Velocity.Dot( wishDir );
		float addspeed = wishSpeed - currentspeed;

		if ( addspeed <= 0 )
			return;

		float accelspeed = accel * Time.Delta * wishSpeed;
		accelspeed = MathF.Min( accelspeed, addspeed );

		Velocity += accelspeed * wishDir;
	}

	private bool CheckJump()
	{
		if ( !IsGrounded )
			return false;

		float jumpVel = JumpVelocity;

		if ( IsGrounded )
		{
			if ( !Input.Jump )
				return false;
		}

		Velocity = Velocity.WithZ( jumpVel );

		SetGroundEntity( null );

		return true;
	}

	private void AirMove()
	{
		ApplyFriction();

		Vector3 wishDir = GetWishDirection();
		float wishSpeed = GetWishSpeed();

		Accelerate( wishDir, wishSpeed, AirControl, AirAcceleration );

		if ( GroundPlane )
		{
			Velocity = ClipVelocity( Velocity, GroundTrace.Normal, Overclip );
		}

		StepSlideMove( true );
	}

	private Vector3 GetWishDirection()
	{
		var vMove = Input.Direction;

		float fMove = vMove.X;
		float sMove = vMove.Y;

		Vector3 forward = Player.EyeRotation.Forward.WithZ( 0 );
		Vector3 right = Player.EyeRotation.Right.WithZ( 0 );

		forward = forward.Normal;
		right = right.Normal;

		Vector3 wishVel = fMove * forward + sMove * right;
		wishVel.Z = 0;

		return wishVel.Normal;
	}

	private float GetWishSpeed()
	{
		if ( !IsGrounded )
			return AirSpeed;

		if ( Sprinting )
			return Speed * 1.5f;

		if ( Crouching )
			return Speed * 0.5f;

		return Speed;
	}

	private void WalkMove()
	{
		ApplyFriction();

		Vector3 wishDir = GetWishDirection();
		float wishSpeed = GetWishSpeed();

		Accelerate( wishDir, wishSpeed, 0, Acceleration );

		// Slide along the ground plane
		float vel = Velocity.Length;
		Velocity = ClipVelocity( Velocity, GroundTrace.Normal, Overclip );

		// Don't decrease velocity when going up or down a slope
		Velocity = Velocity.Normal;
		Velocity *= vel;

		// Don't do anything if standing still
		if ( Velocity.Length <= 0.0001f )
		{
			return;
		}

		StepSlideMove( false );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end )
	{
		return Cast.Ray( start, end ).WithHalfExtents( Player.PlayerHalfExtents ).Ignore( Player ).Run();
	}

	private void TraceToGround()
	{
		Vector3 point = new Vector3( Position ).WithZ( Position.Z - GroundDistance );
		TraceResult trace = TraceBBox( Position, point );
		GroundTrace = trace;

		// do something corrective if the trace starts in a solid...
		if ( trace.StartedSolid )
		{
			LogToScreen( "do something corrective if the trace starts in a solid..." );
			CorrectAllSolid();
		}

		// if the trace didn't hit anything, we are in free fall
		if ( trace.Fraction == 1.0f )
		{
			SetGroundEntity( null );
		}

		// check if getting thrown off the ground
		if ( Velocity.Z > 0 && Velocity.Dot( trace.Normal ) > 10.0f )
		{
			LogToScreen( $"Kickoff" );
			SetGroundEntity( null );
			return;
		}

		// slopes that are too steep will not be considered onground
		var ang = Vector3.GetAngle( Vector3.Up, trace.Normal );
		if ( trace.Entity != null && ang > MaxWalkAngle )
		{
			LogToScreen( $"Too steep" );
			SetGroundEntity( null );

			// If they can't slide down the slope, let them walk
			GroundPlane = true;
			Walking = false;
			return;
		}

		SetGroundEntity( trace );
	}

	private void CategorizePosition()
	{
		// if the player hull point one unit down is solid, the player is on ground
		// see if standing on something solid
		var point = Position + Vector3.Down * 0.01f;
		var bumpPos = Position;

		bool moveToEndPos = false;

		if ( IsGrounded )
		{
			moveToEndPos = true;
			point.Z -= StepSize;
		}

		var trace = TraceBBox( bumpPos, point );

		if ( trace.Entity == null || Vector3.GetAngle( Vector3.Up, trace.Normal ) > MaxWalkAngle )
		{
			SetGroundEntity( null );
			moveToEndPos = false;
		}
		else
		{
			SetGroundEntity( trace );
		}

		if ( moveToEndPos && !trace.StartedSolid && trace.Fraction > 0.0f && trace.Fraction < 1.0f )
		{
			Position = trace.EndPosition;
		}
	}

	private bool CorrectAllSolid()
	{
		Vector3 point;

		LogToScreen( "CorrectAllSolid" );

		{
			for ( int i = -1; i <= 1; i++ )
			{
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int k = -1; k <= 1; k++ )
					{
						point = Position;
						point.X += i;
						point.Y += j;
						point.Z += k - 0.25f;

						var trace = TraceBBox( point, point );

						if ( !trace.StartedSolid )
						{
							LogToScreen( "Found space for correctallsolid" );

							point = Position.WithZ( Position.Z - GroundDistance );
							trace = TraceBBox( Position, point );
							Position = point;
							GroundTrace = trace;

							return true;
						}
					}
				}
			}
		}

		SetGroundEntity( null );
		return false;
	}

	private void SetGroundEntity( TraceResult tr )
	{
		SetGroundEntity( tr.Entity );
	}

	private void SetGroundEntity( BaseEntity ent )
	{
		if ( ent == null )
		{
			GroundPlane = false;
			Walking = false;
		}
		else
		{
			GroundPlane = true;
			Walking = true;
		}

		GroundEntity = ent;
	}
}
