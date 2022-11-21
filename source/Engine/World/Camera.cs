namespace Mocha;

[Category( "Player" ), Title( "Camera" ), Icon( FontAwesome.Camera )]
public class Camera : BaseEntity
{
	// TODO: Native entities (so we can move this bullshit into BaseEntity)
	private Glue.Camera NativeCamera { get; set; }

	private new Vector3 Position
	{
		set => NativeCamera.SetPosition( value );
	}

	public Camera()
	{
		NativeCamera = new();
	}

	public override void Update()
	{
		base.Update();

		// Set camera position
		var newPos = new Vector3();
		newPos.X = MathF.Sin( Time.Now * 3 * 0.5f ) * 4.0f;
		newPos.Y = MathF.Sin( Time.Now * 2 * 0.5f ) * 4.0f;
		newPos.Z = MathF.Cos( Time.Now * 1 * 0.5f ) * 4.0f;
		Position = newPos;
	}
}
