namespace Mocha;

[Category( "Balls" ), Title( "Test Entity" ), Icon( FontAwesome.Vial )]
public partial class TestEntity : ModelEntity
{
	public TestEntity()
	{
		SetModel( "models/guns/ak47.mmdl" );
	}

	public override void Update()
	{
		float time = Time.Now;

		//
		// Spin and move :3
		//
		Position = new Vector3( MathF.Sin( time ),
						 MathF.Cos( time * 2f ),
						 MathF.Sin( time * 0.5f ) );

		Rotation = Rotation.From( time * 90f,
						   time * 30f,
						   time * 10f );
	}
}
