namespace Mocha;

partial class QuakeWalkController
{
	private bool SlideMove( bool gravity )
	{
		Vector3[] planes = new Vector3[5];

		int bumpCount = 0;
		int numPlanes = 0;
		Vector3 endVelocity = new();
		Vector3 clipVelocity = new();
		Vector3 endClipVelocity = new();

		// gravity end
		if ( gravity )
		{
			endVelocity = Velocity;
			endVelocity.Z -= Gravity * Time.Delta * 0.5f;
			Velocity = Velocity.WithZ( (Velocity.Z + endVelocity.Z) * 0.5f );
			LogToScreen( $"SlideMove: {Velocity} -> {endVelocity}" );

			if ( GroundPlane )
			{
				// Slide along the ground plane
				Velocity = ClipVelocity( Velocity, GroundTrace.Normal, Overclip );
				LogToScreen( $"SlideMove GroundPlane: {endVelocity} -> {Velocity}" );
			}
		}

		float timeLeft = Time.Delta;
		float travelFraction = 0;

		// Never turn against the ground plane
		if ( GroundPlane )
		{
			numPlanes = 1;
			planes[0] = GroundTrace.Normal;
		}
		else
		{
			numPlanes = 0;
		}

		// Never turn against original velocity
		planes[numPlanes] = Velocity.Normal;

		for ( bumpCount = 0; bumpCount < 5; bumpCount++ )
		{
			if ( Velocity.Length <= 0.001f )
				break;

			var trace = TraceBBox( Position, Position + Velocity * timeLeft );
			travelFraction += trace.Fraction;

			if ( trace.StartedSolid )
			{
				Velocity = endVelocity.WithZ( 0 );
				return true;
			}

			if ( trace.Fraction > 0.03125f )
			{
				Position = trace.EndPosition;

				if ( trace.Fraction == 1 )
					break;
			}

			timeLeft -= timeLeft * trace.Fraction;

			for ( int i = 0; i < numPlanes; ++i )
			{
				if ( trace.Normal.Dot( planes[i] ) > 0.99f )
				{
					Velocity += trace.Normal;
					break;
				}
			}

			planes[numPlanes] = trace.Normal;

			for ( int i = 0; i < numPlanes; i++ )
			{
				float into = Velocity.Dot( planes[i] );
				if ( into >= 0.1f )
					continue; // Move doesn't interact with the plane

				clipVelocity = ClipVelocity( Velocity, planes[i], Overclip );
				endClipVelocity = ClipVelocity( endClipVelocity, planes[i], Overclip );

				for ( int j = 0; j < numPlanes; j++ )
				{
					if ( j == i )
						continue;

					if ( clipVelocity.Dot( planes[i] ) >= 0 )
						continue;

					clipVelocity = ClipVelocity( clipVelocity, planes[j], Overclip );
					endClipVelocity = ClipVelocity( endClipVelocity, planes[j], Overclip );

					if ( clipVelocity.Dot( planes[i] ) >= 0 )
						continue;

					Vector3 dir = planes[i].Cross( planes[j] ).Normal;
					float d = dir.Dot( Velocity );
					clipVelocity = dir * d;

					dir = planes[i].Cross( planes[j] ).Normal;
					d = dir.Dot( endVelocity );
					endClipVelocity = dir * d;

					for ( int k = 0; k < numPlanes; k++ )
					{
						if ( k == i || k == j )
							continue;

						if ( clipVelocity.Dot( planes[k] ) >= 0.1f )
							continue;

						// Stop dead at a triple plane intersection
						Velocity = Vector3.Zero;
						return true;
					}
				}

				Velocity = clipVelocity;
				endVelocity = endClipVelocity;
			}
		}

		if ( gravity )
			Velocity = endVelocity;

		LogToScreen( $"Bumps: {bumpCount}" );

		return bumpCount != 0;
	}

	private void StepSlideMove( bool gravity )
	{
		var startPos = Position;
		var startVel = Velocity;

		if ( !SlideMove( gravity ) )
		{
			LogToScreen( "SlideMove got exactly where we wanted to go first try" );
			return; // We got exactly where we wanted to go first try
		}

		Vector3 down = startPos;
		down.Z -= StepSize;

		var trace = TraceBBox( startPos, down );
		Vector3 up = new Vector3( 0, 0, 1 );

		// never step up when you still have up velocity
		if ( Velocity.Z > 0 && (trace.Fraction == 1.0f || trace.Normal.Dot( up ) < 0.7f) )
			return;

		up = startPos;
		up.Z += StepSize;

		// test the player position if they were a stepheight higher
		trace = TraceBBox( startPos, up );

		if ( trace.EndedSolid )
		{
			// cant step up
			LogToScreen( $"Can't step up" );
			return;
		}

		float stepSize = trace.EndPosition.Z - startPos.Z;
		Position = trace.EndPosition;
		Velocity = startVel;

		SlideMove( gravity );

		// push down the final amount
		down = Position;
		down.Z -= stepSize;
		trace = TraceBBox( Position, down );

		if ( !trace.EndedSolid )
		{
			Position = trace.EndPosition;
		}

		if ( trace.Fraction < 1.0f )
		{
			Velocity = ClipVelocity( Velocity, trace.Normal, Overclip );
		}
	}
}
