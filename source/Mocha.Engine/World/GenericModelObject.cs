using Veldrid;

namespace Mocha.Engine;

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
			g_mModel = SceneObject.ModelMatrix,
			g_mView = World.Current.Camera.ViewMatrix,
			g_mProj = World.Current.Camera.ProjMatrix,
			g_flTime = Time.Now,

			g_vSunLightDir = World.Current.Sun.Rotation.Forward,
			g_vSunLightColor = World.Current.Sun.Color.ToVector4(),
			g_vSunLightIntensity = World.Current.Sun.Intensity,
			g_vCameraPos = World.Current.Camera.Position,

			_padding1 = 0,
			_padding2 = 0
		};

		models.ForEach( x => x.Draw( uniformBuffer, commandList ) );
	}
}
