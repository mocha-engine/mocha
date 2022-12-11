namespace Mocha;

[Category( "Balls" ), Title( "Test Entity" ), Icon( FontAwesome.Vial )]
public partial class TestEntity : ModelEntity
{
	public TestEntity()
	{
		SetModel( Primitives.Cube.GenerateModel( new Material( "core/materials/dev/dev_wall.mmat" ) ) );
	}

	public override void Update()
	{
		float time = Time.Now;

		//
		// Spin and move
		//
		Position = new Vector3( (MathF.Cos( time * 2f )).Clamp( 0, 1 ) * 10f, 0,
						 0
						 );

		Rotation = Rotation.From( time * 90f,
						   time * 30f,
						   time * 10f );
	}
}
