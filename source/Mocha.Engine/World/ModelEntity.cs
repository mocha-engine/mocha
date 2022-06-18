using Veldrid;

namespace Mocha.Engine;

public partial class ModelEntity : Entity
{
	public SceneObject SceneObject { get; set; }

	public ModelEntity( string modelPath )
	{
		SceneObject = new ModelSceneObject( this )
		{
			models = Primitives.MochaModel.GenerateModels( modelPath )
		};
	}
}
