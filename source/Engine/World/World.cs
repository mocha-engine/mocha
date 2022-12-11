namespace Mocha;

public class World
{
	public static World Current { get; set; }

	public World()
	{
		Current = this;
		Event.Register( this );
		Event.RegisterStatics();
		Event.Run( Event.Game.LoadAttribute.Name );

		SetupEntities();
	}

	private void SetupEntities()
	{
		Log.Trace( $"Setting up entities..." );

		var floor = new ModelEntity( "core/models/dev/dev_map.mmdl" );
		floor.Position = new Vector3( 0, 0, -0.5f );
		floor.SetCubePhysics( new Vector3( 19f, 19f, 0.5f ), true );

		for ( int x = -4; x < 4; ++x )
		{
			for ( int y = -4; y < 4; y++ )
			{
				var ball = new ModelEntity( "core/models/dev/dev_ball.mmdl" );
				ball.Position = new( x, y, 10.0f + (x + y) );
				ball.SetSpherePhysics( 0.5f, false );
			}
		}

		var player = new Player();
	}

	public void Update()
	{
		BaseEntity.All.ForEach( entity => entity.Update() );
	}
}
