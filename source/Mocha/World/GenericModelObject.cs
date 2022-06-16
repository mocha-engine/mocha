using Veldrid;

namespace Mocha;

public partial class GenericModelObject : Entity
{
	private List<Model> models;

	public GenericModelObject( string modelPath )
	{
		models = Primitives.MochaModel.GenerateModels( modelPath );
	}

	public override void Render( CommandList commandList )
	{
		base.Render( commandList );

		var uniformBuffer = new GenericModelUniformBuffer
		{
			g_mModel = ModelMatrix,
			g_mView = World.Current.Camera.ViewMatrix,
			g_mProj = World.Current.Camera.ProjMatrix,
			g_flTime = Time.Now,

			g_vSunLightDir = World.Current.Sun.rotation.Forward,
			g_vSunLightColor = World.Current.Sun.Color,
			g_vSunLightIntensity = World.Current.Sun.Intensity,
			g_vCameraPos = World.Current.Camera.position,

			_padding1 = 0,
			_padding2 = 0,
			_padding3 = 0
		};

		models.ForEach( x => x.Draw( uniformBuffer, commandList ) );
	}
}
