﻿namespace Mocha;

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

#if DEBUG
	public States State { get; set; } = States.Paused;
#else
	public States State { get; set; } = States.Playing;
#endif

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

		Camera = new Camera();

		Sun = new Sun()
		{
			Position = new( 20, 25, 80 ),
			Rotation = Rotation.From( 27, 15, 0 )
		};

		_ = new ModelEntity( "models/subaru/subaru.mmdl" );
	}

	public void Update()
	{
		if ( State == States.Playing )
			BaseEntity.All.ForEach( entity => entity.Update() );
	}
}
