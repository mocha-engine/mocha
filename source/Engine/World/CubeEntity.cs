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
		Position = new Vector3( MathF.Sin( Time.Now ), MathF.Cos( Time.Now * 2f ), MathF.Sin( Time.Now * 0.5f ) );

		// Adjust our rotation over time
		Rotation = Rotation.From( Time.Now * 90f, Time.Now * 30f, Time.Now * 10f );
	}
}
