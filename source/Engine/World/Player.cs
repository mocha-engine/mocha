namespace Mocha;

[Category( "Player" ), Icon( FontAwesome.User )]
public class Player : ModelEntity
{
	// Get local player instance
	public static Player Local => BaseEntity.All.OfType<Player>().First();

	public Vector3 EyePosition => Position + Vector3.Up * 0.75f;
	public Rotation EyeRotation => Input.Rotation;
	public Ray EyeRay => new Ray( EyePosition, EyeRotation.Forward );

	protected override void Spawn()
	{
		base.Spawn();

		Position = new Vector3( -4, 0, 4 );

		Restitution = 0.0f;
		Friction = 1.0f;
		Mass = 100f;
		IgnoreRigidbodyRotation = true;

		SetCubePhysics( new Vector3( 0.25f, 0.25f, 0.75f ), false );
	}

	public override void Update()
	{
		UpdateCamera();

		if ( Input.Left )
		{
			Velocity += (Input.Rotation.Forward * Time.Delta * 10f).WithZ( 0 );
		}
	}

	private void UpdateCamera()
	{
		Camera.Rotation = EyeRotation;
		Camera.Position = EyePosition;
		Camera.FieldOfView = 90f;
		Camera.ZNear = 0.1f;
		Camera.ZFar = 1000.0f;
	}
}
