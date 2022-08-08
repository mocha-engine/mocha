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
			model = new Model( modelPath )
		};
	}

	public override void Update()
	{
		base.Update();

		SceneObject.Transform = Transform;
	}
}
