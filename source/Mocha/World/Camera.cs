using System.Numerics;

namespace Mocha;

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

	private void CalcViewProjMatrix()
	{
		var cameraPos = position;

		var direction = new Vector3(
			MathF.Cos( rotation.Y.DegreesToRadians() ) * MathF.Cos( rotation.X.DegreesToRadians() ),
			MathF.Sin( rotation.Y.DegreesToRadians() ) * MathF.Cos( rotation.X.DegreesToRadians() ),
			MathF.Sin( rotation.X.DegreesToRadians() )
		);
		var cameraFront = direction;

		var cameraUp = new Vector3( 0, 0, 1 );

		ViewMatrix = Matrix4x4.CreateLookAt( cameraPos, cameraPos + cameraFront, cameraUp );
		ProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
			90.0f.DegreesToRadians(),
			Screen.Aspect,
			0.1f,
			1000.0f
		);
	}

	private bool wasPressed = false;

	public override void Update()
	{
		base.Update();

		//
		// Get user input
		//

		var wishDir = new Vector3( Input.Forward, Input.Left, 0 ).Normal;

		wasPressed = Input.MouseRight;

		wishVelocity = Forward * wishDir.X * Time.Delta * cameraSpeed;
		wishVelocity += Right * wishDir.Y * Time.Delta * cameraSpeed;

		if ( Input.Down( InputButton.Jump ) )
			wishVelocity.Z += cameraSpeed * Time.Delta;

		if ( Input.Down( InputButton.Sprint ) )
			wishVelocity *= 4.0f;

		rotation.Y -= Input.MouseDelta.X * 20f * Time.Delta;
		rotation.X -= Input.MouseDelta.Y * 20f * Time.Delta;

		rotation.X = rotation.X.Clamp( -89, 89 );

		//
		// Apply everything
		//

		// Apply velocity
		velocity += wishVelocity;

		// Apply drag
		velocity *= 1 - Time.Delta * 10f;

		// Move camera
		position += velocity * Time.Delta;

		// Run view/proj matrix calculations
		CalcViewProjMatrix();
	}
}
