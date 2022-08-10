using Mocha.Common.World;

namespace Mocha.Renderer;

public class SceneCamera : SceneObject
{
	public Matrix4x4 ViewMatrix { get; set; }
	public Matrix4x4 ProjMatrix { get; set; }

	public float FieldOfView { get; set; } = 60;

	private Point2 CurrentSize { get; set; }

	public SceneCamera( IEntity entity ) : base( entity )
	{
	}

	public void UpdateAspect( Point2 newSize )
	{
		if ( newSize.X == CurrentSize.X && newSize.Y == CurrentSize.Y )
			return;

		CurrentSize = newSize;
	}

	private void UpdateSize( Point2 newSize )
	{
		if ( newSize.X == CurrentSize.X && newSize.Y == CurrentSize.Y )
			return;

		Log.Trace( $"Resized CAMERA to {newSize}" );

		CurrentSize = newSize;
	}

	public void CalcViewProjMatrix()
	{
		FieldOfView = FieldOfView.Clamp( 1, 179 );

		var cameraPos = Transform.Position;
		var cameraFront = Transform.Rotation.Forward;
		var cameraUp = new Vector3( 0, 0, 1 );

		ViewMatrix = Matrix4x4.CreateLookAt( cameraPos, cameraPos + cameraFront, cameraUp );
		ProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
			FieldOfView.DegreesToRadians(),
			(float)CurrentSize.X / (float)CurrentSize.Y,
			0.1f,
			1000.0f
		);
	}
}
