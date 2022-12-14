namespace Mocha;

[Category( "Player" ), Icon( FontAwesome.User )]
public class Player : ModelEntity
{
	// Get local player instance
	public static Player Local => BaseEntity.All.OfType<Player>().First();

	private Vector3 PlayerBounds = new( 0.5f, 0.5f, 1.8f ); // Metres
	public Vector3 PlayerHalfExtents => PlayerBounds / 2f;

	public Vector3 EyePosition => Position + Vector3.Up * PlayerHalfExtents.Z;
	public Rotation EyeRotation => Input.Rotation;
	public Ray EyeRay => new Ray( EyePosition, EyeRotation.Forward );

	private QuakeWalkController WalkController { get; set; }

	public bool IsGrounded => WalkController.IsGrounded;
	public BaseEntity GroundEntity => WalkController.GroundEntity;

	protected override void Spawn()
	{
		base.Spawn();

		Restitution = 0.0f;
		Friction = 1.0f;
		Mass = 100f;
		IgnoreRigidbodyRotation = true;

		Respawn();
	}

	bool rightLastFrame = false;

	public void Respawn()
	{
		WalkController = new( this );
		Velocity = Vector3.Zero;
		Position = new Vector3( 0, 0, 2.0f );
	}

	public override void Update()
	{
		UpdateCamera();
		WalkController.Update();

		//
		// Spawn some balls when right clicking
		//
		if ( Input.Right && !rightLastFrame )
		{
			var tr = Cast.Ray( EyeRay, 10f ).Ignore( this ).Run();

			var ball = new ModelEntity( "core/models/dev/dev_ball.mmdl" );
			ball.Name = "My Ball";
			ball.Position = tr.EndPosition + tr.Normal * 1.0f;
			ball.Restitution = 1.0f;
			ball.Friction = 1.0f;
			ball.Mass = 10.0f;
			ball.SetSpherePhysics( 0.5f, false );
		}

		rightLastFrame = Input.Right;
	}

	private void UpdateCamera()
	{
		Camera.Rotation = EyeRotation;
		Camera.Position = EyePosition;
		Camera.FieldOfView = 90f;
		Camera.ZNear = 0.01f;
		Camera.ZFar = 1000.0f;
	}
}
