namespace Mocha.Common;

public struct CameraSetup
{
	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }

	public float AspectRatio { get; set; }
	
	public float ZFar { get; set; }
	public float ZNear { get; set; }

	public float FieldOfView { get; set; }

	public void BuildMatrices( out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix )
	{
		FieldOfView = FieldOfView.Clamp( 1, 179 );

		var cameraPos = Position;
		var cameraFront = Rotation.Forward;
		var cameraUp = new Vector3( 0, 0, 1 );

		viewMatrix = Matrix4x4.CreateLookAt( cameraPos, cameraPos + cameraFront, cameraUp );
		projMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
			FieldOfView.DegreesToRadians(),
			AspectRatio,
			0.1f,
			1000.0f
		);
	}
}
