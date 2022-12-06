namespace Mocha;

[Category( "Player" ), Title( "Camera" ), Icon( FontAwesome.Camera )]
public class Camera : BaseEntity
{
	public override void Update()
	{
		base.Update();

		// Set camera position
		var newPos = new Vector3();
		newPos.X = MathF.Sin( Time.Now * 3 * 0.5f ) * 4.0f;
		newPos.Y = MathF.Sin( Time.Now * 2 * 0.5f ) * 4.0f;
		newPos.Z = MathF.Cos( Time.Now * 1 * 0.5f ) * 4.0f;

		Position = newPos;

		Position = new Vector3( 0, 0, -4 );
		Glue.Entities.SetCameraPosition( Position );
	}
}
