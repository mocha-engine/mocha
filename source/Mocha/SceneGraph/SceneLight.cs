namespace Mocha.Renderer;

public class SceneLight : SceneObject
{
	public float Intensity { get; set; } = 1.0f;
	public RgbaFloat Color { get; set; } = new RgbaFloat( 1, 1, 1, 1 );

	internal Framebuffer ShadowBuffer { get; set; }
	internal Texture DepthTexture { get; set; }

	internal Matrix4x4 ViewMatrix { get; set; }
	internal Matrix4x4 ProjMatrix { get; set; }

	public SceneLight( IEntity entity ) : base( entity )
	{
		DepthTexture = Texture.Builder
			.FromEmpty( 1024, 1024 )
			.AsDepthAttachment()
			.Build();

		var framebufferDescription = new FramebufferDescription( DepthTexture.VeldridTexture );
		ShadowBuffer = Device.ResourceFactory.CreateFramebuffer( framebufferDescription );
	}

	public void CalcViewProjMatrix()
	{
		var cameraPos = Transform.Position;
		var cameraFront = Transform.Rotation.Forward;
		var cameraUp = new Vector3( 0, 0, 1 );

		ViewMatrix = Matrix4x4.CreateLookAt( cameraPos, cameraPos + cameraFront, cameraUp );
		ProjMatrix = Matrix4x4.CreateOrthographic( 20f, 20f, -10f, 20f );
	}
}
