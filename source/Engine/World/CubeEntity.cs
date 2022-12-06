namespace Mocha;

[Category( "World" ), Title( "Cube Entity" ), Icon( FontAwesome.Cube )]
public partial class CubeEntity : ModelEntity
{
	public CubeEntity()
	{
		// Generate a cube on the fly in C#
		var model = Primitives.Cube.GenerateModel( new Material() );

		// Pass the model over to C++
		SetModel( model );
	}

	public override void Update()
	{
		// Adjust our position over time - to make sure Update works
		Position = new Vector3( MathF.Sin( Time.Now ), 0, 0 );
	}
}
