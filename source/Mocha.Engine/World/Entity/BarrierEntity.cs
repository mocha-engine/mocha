namespace Mocha.Engine;

public class BarrierEntity : ModelEntity
{
	public float Offset { get; set; }

	public BarrierEntity() : base( "content/models/barrier.mmdl" )
	{
		Rotation = Rotation.From( 0, 0, 90 );
		Scale = new Vector3( 0.025f );

		Spawn();
	}

	private void Spawn()
	{
		Position = new( 32, -World.Bounds, 0 );

		var zPos = Random.Shared.Next( -8, 8 );
		Position = Position.WithZ( zPos );
	}

	public override void Update()
	{
		base.Update();

		Position = Position.WithY( Position.Y + Time.Delta * 32 );

		if ( Position.Y > World.Bounds )
		{
			Spawn();
		}

		if ( Position.Y < -World.Bounds )
		{
			Spawn();
		}
	}
}
