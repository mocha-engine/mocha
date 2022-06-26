using Veldrid;

namespace Mocha.Renderer;

public class SceneLight : SceneObject
{
	public float Intensity { get; set; } = 1.0f;
	public RgbaFloat Color { get; set; } = new RgbaFloat( 1, 1, 1, 1 );

	internal Framebuffer ShadowBuffer { get; set; }
	internal Texture DepthTexture { get; set; }

	internal OutputDescription Output { get; set; }

	internal Matrix4x4 ViewMatrix { get; set; }
	internal Matrix4x4 ProjMatrix { get; set; }

	public SceneLight( IEntity entity ) : base( entity )
	{
		uint res = 8192;

		DepthTexture = Texture.Builder
			.FromEmpty( res, res )
			.AsDepthAttachment()
			.WithName( "Sun Light Depth" )
			.Build();

		var framebufferDescription = new FramebufferDescription( DepthTexture.VeldridTexture );

		ShadowBuffer = Device.ResourceFactory.CreateFramebuffer( framebufferDescription );
	}

	public void CalcViewProjMatrix()
	{
		ViewMatrix = Matrix4x4.CreateLookAt( Transform.Position, Vector3.Zero, Vector3.Up );
		ProjMatrix = Matrix4x4.CreateOrthographic( 100f, 100f, 1.0f, 200f );
	}
}
