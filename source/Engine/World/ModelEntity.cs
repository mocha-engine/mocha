namespace Mocha;

[Category( "World" ), Title( "Model Entity" ), Icon( FontAwesome.Cube )]
public partial class ModelEntity : BaseEntity
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
