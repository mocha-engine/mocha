using Veldrid;

namespace Mocha;

public class Sky : Entity
{
	private Model Model { get; set; }
	private Material Material { get; set; }

	public Sky()
	{
		Material = new()
		{
			Shader = ShaderBuilder.Default.FromMoyaiShader( "content/shaders/atmosphere.mshdr" ).Build(),
			UniformBufferType = typeof( GenericModelUniformBuffer )
		};

		Model = Primitives.Cube.GenerateModel( Material );
	}

	public override void Render( CommandList commandList )
	{
		position = World.Current.Camera.position;
		var uniformBuffer = new GenericModelUniformBuffer
		{
			g_mModel = ModelMatrix,
			g_mView = World.Current.Camera.ViewMatrix,
			g_mProj = World.Current.Camera.ProjMatrix,
			g_vLightPos = World.Current.Sun.position,
			g_fTime = Time.Now,
			g_vLightColor = World.Current.Sun.Color,
			g_vCameraPos = World.Current.Camera.position,

			_padding1 = 0,
			_padding2 = 0
		};

		Model.Draw( uniformBuffer, commandList );
	}
}
