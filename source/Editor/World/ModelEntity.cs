using System.ComponentModel;

namespace Mocha.Editor;

[Category( "World" ), Title( "Model Entity" ), Icon( FontAwesome.Cube )]
public partial class ModelEntity : Entity
{
	[HideInInspector]
	public SceneObject SceneObject { get; set; }

	public ModelEntity( string modelPath )
	{
		SceneObject = new ModelSceneObject( this )
		{
			models = Primitives.MochaModel.GenerateModels( modelPath )
		};
	}
}
