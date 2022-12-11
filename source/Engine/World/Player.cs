namespace Mocha;

[Category( "Player" ), Icon( FontAwesome.User )]
public class Player : ModelEntity
{
	public override void Update()
	{
		UpdateCamera();
	}

	private void UpdateCamera()
	{
		Camera.Rotation = Rotation.Identity;
		Camera.Position = new Vector3( -4, 0, 2 );
		Camera.FieldOfView = 90f;
		Camera.ZNear = 0.1f;
		Camera.ZFar = 1000.0f;
	}
}
