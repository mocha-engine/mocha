namespace Mocha;

[Category( "Player" ), Title( "Camera" ), Icon( FontAwesome.Camera )]
public class Camera : BaseEntity
{
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

		// Run view/proj matrix calculations
		SceneCamera.CalcViewProjMatrix();
	}
}
