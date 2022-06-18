namespace Mocha.Renderer;

public class SceneCamera : SceneObject
{
	public Matrix4x4 ViewMatrix { get; set; }
	public Matrix4x4 ProjMatrix { get; set; }

	public float FieldOfView { get; set; } = 60;

	public SceneCamera( IEntity entity ) : base( entity )
	{
	}

	public void CalcViewProjMatrix()
	{
		var cameraPos = Transform.Position;
		var cameraFront = Transform.Rotation.Forward;
		var cameraUp = new Vector3( 0, 0, 1 );

		ViewMatrix = Matrix4x4.CreateLookAt( cameraPos, cameraPos + cameraFront, cameraUp );
		ProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
			FieldOfView.DegreesToRadians(),
			Screen.Aspect,
			0.1f,
			1000.0f
		);
	}
}
