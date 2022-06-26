using Veldrid;

namespace Mocha.Renderer;

public class SceneLight : SceneObject
{
	public float Intensity { get; set; } = 1.0f;
	public RgbaFloat Color { get; set; } = new RgbaFloat( 1, 1, 1, 1 );

	internal Framebuffer ShadowBuffer { get; set; }
	internal Texture DepthTexture { get; set; }
	internal Texture PositionTexture { get; set; }
	internal Texture NormalTexture { get; set; }
	internal Texture FluxTexture { get; set; }

	internal OutputDescription Output { get; set; }

	internal Matrix4x4 ViewMatrix { get; set; }
	internal Matrix4x4 ProjMatrix { get; set; }

	public SceneLight( IEntity entity ) : base( entity )
	{
		uint res = 1024;

		DepthTexture = Texture.Builder
			.FromEmpty( res, res )
			.AsDepthAttachment()
			.WithName( "Sun Light Depth" )
			.Build();

		PositionTexture = Texture.Builder
			.FromEmpty( res, res )
			.AsColorAttachment()
			.WithName( "Sun Light Position" )
			.Build();

		NormalTexture = Texture.Builder
			.FromEmpty( res, res )
			.AsColorAttachment()
			.WithName( "Sun Light Normal" )
			.Build();

		FluxTexture = Texture.Builder
			.FromEmpty( res, res )
			.AsColorAttachment()
			.WithName( "Sun Light Flux" )
			.Build();

		var framebufferDescription = new FramebufferDescription( DepthTexture.VeldridTexture,
			PositionTexture.VeldridTexture, NormalTexture.VeldridTexture, FluxTexture.VeldridTexture );

		ShadowBuffer = Device.ResourceFactory.CreateFramebuffer( framebufferDescription );
	}

	public void CalcViewProjMatrix()
	{
		ViewMatrix = Matrix4x4.CreateLookAt( Transform.Position, Vector3.Zero, Vector3.Up );
		ProjMatrix = Matrix4x4.CreateOrthographic( 400f, 400f, 1.0f, 200f );
	}
}
