namespace Mocha.Engine;

[Category( "World" ), Title( "Model Entity" ), Icon( FontAwesome.Cube )]
public partial class ModelEntity : Entity
{
	public SceneObject SceneObject { get; set; }

	public bool Visible { get; set; } = true;

	public ModelEntity( string modelPath )
	{
		SceneObject = new ModelSceneObject()
		{
			models = Primitives.MochaModel.GenerateModels( modelPath )
		};
	}

	public override void Update()
	{
		base.Update();

		SceneObject.Transform = Transform;
	}
}
