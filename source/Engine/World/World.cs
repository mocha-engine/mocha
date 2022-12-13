﻿namespace Mocha;

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
		floor.Position = new Vector3( -4.0f, 0, 2.0f );
		floor.Friction = 1.0f;
		floor.Restitution = 0.0f;
		floor.Mass = 1000.0f;
		floor.SetMeshPhysics( "core/models/dev/dev_map.mmdl" );

		//for ( int x = -4; x < 4; ++x )
		//{
		//	for ( int y = -4; y < 4; y++ )
		//	{
		//		var ball = new ModelEntity( "core/models/dev/dev_ball.mmdl" );
		//		ball.Position = new( x, y, 10.0f + (x + y) );
		//		ball.Restitution = 1.0f;
		//		ball.Friction = 1.0f;
		//		ball.Mass = 10.0f;
		//		ball.SetSpherePhysics( 0.5f, false );
		//	}
		//}

		var player = new Player();
	}

	public void Update()
	{
		BaseEntity.All.ToList().ForEach( entity => entity.Update() );
	}
}
