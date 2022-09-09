using System.ComponentModel;

namespace Mocha.Engine;

[Category( "Player" ), Title( "Camera" ), Icon( FontAwesome.Camera )]
public class Camera : Entity
{
	[HideInInspector] private SceneCamera SceneCamera { get; set; }

	public float FieldOfView { get; set; } = 60;

	public Camera()
	{
		SceneCamera = new( this );
	}

	public override void BuildCamera( ref CameraSetup cameraSetup )
	{
		base.BuildCamera( ref cameraSetup );

		cameraSetup.Position = Position;
		cameraSetup.Rotation = Rotation;

		cameraSetup.FieldOfView = FieldOfView;

		cameraSetup.ZNear = 0.1f;
		cameraSetup.ZFar = 1000f;
	}
}
