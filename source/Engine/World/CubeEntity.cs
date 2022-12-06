namespace Mocha;

[Category( "World" ), Title( "Cube Entity" ), Icon( FontAwesome.Cube )]
public partial class CubeEntity : ModelEntity
{
	public CubeEntity()
	{
		var model = Primitives.Cube.GenerateModel( new Material() );
		SetModel( model );
	}
}
