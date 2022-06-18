namespace Mocha.Engine;

public class Camera : Entity
{
	public SceneCamera SceneCamera { get; set; }

	private Vector3 velocity = new();
	private Vector3 wishVelocity = new();

	private float cameraSpeed = 100f;

	private float FieldOfView
	{
		get => SceneCamera.FieldOfView;
		set => SceneCamera.FieldOfView = value;
	}

	private float wishFov = 90f;

	public Matrix4x4 ProjMatrix => SceneCamera.ProjMatrix;
	public Matrix4x4 ViewMatrix => SceneCamera.ViewMatrix;

	public Camera()
	{
		SceneCamera = new( this );
	}

	public override void Update()
	{
		base.Update();

		//
		// Get user input
		//
		var wishDir = new Vector3( Input.Forward, Input.Left, 0 ).Normal;

		wishVelocity = Rotation.Forward * wishDir.X * Time.Delta * cameraSpeed;
		wishVelocity += Rotation.Right * wishDir.Y * Time.Delta * cameraSpeed;

		if ( Input.Down( InputButton.Jump ) )
			wishVelocity.Z += cameraSpeed * Time.Delta;

		if ( Input.Down( InputButton.Sprint ) )
			wishVelocity *= 4.0f;

		var targetEuler = Rotation.ToEulerAngles();
		targetEuler.X += Input.MouseDelta.Y * 20f * Time.Delta;
		targetEuler.Y -= Input.MouseDelta.X * 20f * Time.Delta;

		targetEuler.X = targetEuler.X.Clamp( -89, 89 );
		Rotation = Rotation.From( targetEuler.X, targetEuler.Y, targetEuler.Z );

		float t = velocity.WithZ( 0 ).Length.LerpInverse( 0, 50 );
		wishFov = 60f.LerpTo( 90f, t );

		//
		// Apply everything
		//

		// Apply velocity
		velocity += wishVelocity;

		// Apply drag
		velocity *= 1 - Time.Delta * 10f;

		// Move camera
		Position += velocity * Time.Delta;

		// Apply fov
		FieldOfView = FieldOfView.LerpTo( wishFov, 5f * Time.Delta );

		// Run view/proj matrix calculations
		SceneCamera.CalcViewProjMatrix();
	}
}
