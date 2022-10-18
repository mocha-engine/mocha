namespace Mocha.Engine;

public class World
{
	public static World Current { get; set; }
	public static float Bounds => 48f;

	public Camera? Camera { get; set; }
	public Sun? Sun { get; set; }
	public Sky? Sky { get; set; }
	public Player? Player { get; set; }

	public enum States
	{
		Playing,
		Paused
	}

	public States State { get; set; } = States.Playing;

	public World()
	{
		Current = this;
		Event.Register( this );
		Event.RegisterStatics();
		Event.Run( Event.Game.HotloadAttribute.Name );

		SetupEntities();
	}

	private void SetupEntities()
	{
		Log.Trace( $"Setting up entities..." );

		Sun = new Sun()
		{
			Position = new( 20, 25, 80 ),
			Rotation = Rotation.From( 27, 15, 0 )
		};

		_ = new ModelEntity( "core/models/dev/dev_map.mmdl" )
		{
			Scale = new( 0.3937f ),
			Rotation = Rotation.From( 0, 0, 90 )
		};

		//Sky = new Sky
		//{
		//    Scale = Vector3.One * -10000f
		//};

		Player = new Player();
		Camera = new Camera() { Player = Player };
	}

	public void Update()
	{
		foreach ( var entity in Entity.All )
		{
			if ( State == States.Playing || entity is Camera )
				entity.Update();
		}
	}

	public Entity FindActiveCamera()
	{
		return Entity.All.OfType<Camera>().First();
	}
}
