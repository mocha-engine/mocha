namespace Mocha.Engine;

public class Camera : Entity
{
	public Matrix4x4 ViewMatrix { get; set; }
	public Matrix4x4 ProjMatrix { get; set; }

	public Vector3 Forward => ViewMatrix.Forward();
	public Vector3 Right => ViewMatrix.Right();
	public Vector3 Up => ViewMatrix.Up();

	private Vector3 velocity = new();
	private Vector3 wishVelocity = new();

	private float cameraSpeed = 100f;

	private float fov = 90f;
	private float wishFov = 90f;

	private void CalcViewProjMatrix()
	{
		var cameraPos = Position;

		// TODO: Do this proper
		var direction = new Vector3(
			MathF.Cos( Rotation.Y.DegreesToRadians() ) * MathF.Cos( Rotation.X.DegreesToRadians() ),
			MathF.Sin( Rotation.Y.DegreesToRadians() ) * MathF.Cos( Rotation.X.DegreesToRadians() ),
			MathF.Sin( Rotation.X.DegreesToRadians() )
		);
		var cameraFront = direction;

		var cameraUp = new Vector3( 0, 0, 1 );

		ViewMatrix = Matrix4x4.CreateLookAt( cameraPos, cameraPos + cameraFront, cameraUp );
		ProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
			fov.DegreesToRadians(),
			Screen.Aspect,
			0.1f,
			1000.0f
		);
	}

	public override void Update()
	{
		base.Update();

		//
		// Get user input
		//
		var wishDir = new Vector3( Input.Forward, Input.Left, 0 ).Normal;

		wishVelocity = Forward * wishDir.X * Time.Delta * cameraSpeed;
		wishVelocity += Right * wishDir.Y * Time.Delta * cameraSpeed;

		if ( Input.Down( InputButton.Jump ) )
			wishVelocity.Z += cameraSpeed * Time.Delta;

		if ( Input.Down( InputButton.Sprint ) )
			wishVelocity *= 4.0f;

		var targetRot = Rotation;
		targetRot.Y -= Input.MouseDelta.X * 20f * Time.Delta;
		targetRot.X -= Input.MouseDelta.Y * 20f * Time.Delta;

		targetRot.X = Rotation.X.Clamp( -89, 89 );
		Rotation = targetRot;

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
		fov = fov.LerpTo( wishFov, 5f * Time.Delta );

		// Run view/proj matrix calculations
		CalcViewProjMatrix();
	}
}
