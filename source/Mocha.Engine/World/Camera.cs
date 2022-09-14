namespace Mocha.Engine;

[Category( "Player" ), Title( "Camera" ), Icon( FontAwesome.Camera )]
public class Camera : Entity
{
	public float FieldOfView { get; set; } = 60;
	public Player Player { get; set; }

	public Vector3 Velocity { get; set; }

	public Vector3 Forward => Vector3.Forward;
	public Vector3 Left => Vector3.Left;

	private float Speed => 500f;
	private float Accel => 500f;

	public override void Update()
	{
		base.Update();

		Velocity += ((Input.Left * Vector3.Left) + (Input.Forward * Vector3.Forward))
			* Accel * Time.Delta;

		if ( Velocity.Length > Speed )
			Velocity = Velocity.Normal * Speed;

		Velocity = Velocity.LerpTo( Vector3.Zero, Time.Delta * 5f );

		Position += Velocity * Time.Delta;
	}

	public override void BuildCamera( ref CameraSetup cameraSetup )
	{
		base.BuildCamera( ref cameraSetup );

		cameraSetup.Position = Player.Position;
		cameraSetup.Position -= Player.Rotation.Up * 32f;
		cameraSetup.Position += Vector3.Up * 8f;

		var lookAt = cameraSetup.Position - Player.Position;
		cameraSetup.Rotation = Rotation.LookAt( lookAt );

		cameraSetup.FieldOfView = FieldOfView;

		cameraSetup.ZNear = 0.1f;
		cameraSetup.ZFar = 1000f;
	}
}
