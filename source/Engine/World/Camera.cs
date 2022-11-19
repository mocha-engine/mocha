namespace Mocha;

[Category( "Player" ), Title( "Camera" ), Icon( FontAwesome.Camera )]
public class Camera : BaseEntity
{
	private new Vector3 Position
	{
		set => NativeCamera.SetPosition( value );
	}

	private Glue.Camera NativeCamera { get; set; }

	[HideInInspector]
	public SceneCamera SceneCamera { get; set; }

	private float FieldOfView
	{
		get => SceneCamera.FieldOfView;
		set => SceneCamera.FieldOfView = value;
	}

	[HideInInspector]
	public Matrix4x4 ProjMatrix => SceneCamera.ProjMatrix;

	[HideInInspector]
	public Matrix4x4 ViewMatrix => SceneCamera.ViewMatrix;

	public Camera()
	{
		NativeCamera = new();
		SceneCamera = new( this );
	}

	public override void Update()
	{
		base.Update();

		// Apply fov
		FieldOfView = 50f;

		// Set camera position
		var newPos = new Vector3();
		newPos.X = MathF.Sin( Time.Now * 3 * 0.5f ) * 4.0f;
		newPos.Y = MathF.Sin( Time.Now * 2 * 0.5f ) * 4.0f;
		newPos.Z = MathF.Cos( Time.Now * 1 * 0.5f ) * 4.0f;
		Position = newPos;

		// Run view/proj matrix calculations
		SceneCamera.CalcViewProjMatrix();
	}
}
