namespace Mocha;

[Category( "World" ), Title( "Cube Entity" ), Icon( FontAwesome.Cube )]
public partial class CubeEntity : BaseEntity
{
	[HideInInspector]
	public SceneObject SceneObject { get; set; }

	public CubeEntity()
	{
		SceneObject = new ModelSceneObject( this )
		{
			models = new() { Primitives.Cube.GenerateModel( new Material() ) }
		};
	}
}
