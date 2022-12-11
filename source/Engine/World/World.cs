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
		floor.Position = new Vector3( 0, 256f, 0 );

		var ball = new ModelEntity( "core/models/dev/dev_ball.mmdl" );

		var player = new Player();
	}

	public void Update()
	{
		BaseEntity.All.ForEach( entity => entity.Update() );
	}
}
