namespace Mocha;

[Category( "Player" ), Icon( FontAwesome.User )]
public class Player : ModelEntity
{
	// Get local player instance
	[HideInInspector]
	public static Player Local => BaseEntity.All.OfType<Player>().First();

	private Vector3 PlayerBounds = new( 0.5f, 0.5f, 1.8f ); // Metres
	[Category( "Player" )]
	public Vector3 PlayerHalfExtents => PlayerBounds / 2f;

	[Category( "Player" )]
	public Vector3 EyePosition => Position + Vector3.Up * PlayerHalfExtents.Z;
	[Category( "Player" )]
	public Rotation EyeRotation => Input.Rotation;
	[HideInInspector]
	public Ray EyeRay => new Ray( EyePosition, EyeRotation.Forward );

	[Category( "Player" )]
	public QuakeWalkController WalkController { get; private set; }

	[Category( "Player" )]
	public bool IsGrounded => WalkController.IsGrounded;
	[Category( "Player" )]
	public BaseEntity GroundEntity => WalkController.GroundEntity;

	[Category( "Player" )]
	public Vector3 LocalEyePosition { get; set; }
	[Category( "Player" )]
	public Rotation LocalEyeRotation { get; set; }

	[Category( "Player" )]
	public ViewModel ViewModel { get; set; }

	protected override void Spawn()
	{
		base.Spawn();

		Restitution = 0.0f;
		Friction = 1.0f;
		Mass = 100f;
		IgnoreRigidbodyRotation = true;

		// ViewModel = new();
		Respawn();
	}

	public void Respawn()
	{
		WalkController = new( this );
		Velocity = Vector3.Zero;
		Position = new Vector3( 0.0f, 4.0f, 0.9f );
	}

	private TimeSince TimeSinceFire;

	public override void Update()
	{
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
