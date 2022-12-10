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
		Camera.Position = new Vector3( 0, 0, -64 );
		Camera.FieldOfView = 90f + MathF.Sin( Time.Now ) * 30f;
		Camera.ZNear = 1f;
		Camera.ZFar = 1000.0f;
	}
}
