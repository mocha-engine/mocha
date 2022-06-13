using Veldrid;

namespace Mocha;

public partial class GenericModelObject : Entity
{
	private List<Model> models;

	public GenericModelObject( string modelPath )
	{
		models = Primitives.Assimp.GenerateModels( modelPath );
	}

	public override void Render( CommandList commandList )
	{
		base.Render( commandList );

		var uniformBuffer = new GenericModelUniformBuffer
		{
			g_mModel = ModelMatrix,
			g_mView = World.Current.Camera.ViewMatrix,
			g_mProj = World.Current.Camera.ProjMatrix,
			g_vLightPos = World.Current.Sun.position,
			g_vLightColor = World.Current.Sun.Color,
			g_vCameraPos = World.Current.Camera.position,

			_padding0 = 0,
			_padding1 = 0,
			_padding2 = 0
		};

		models.ForEach( x => x.Draw( uniformBuffer, commandList ) );
	}
}
