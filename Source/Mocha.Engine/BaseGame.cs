namespace Mocha;

public class BaseGame : IGame
{
	public static BaseGame Current { get; set; }

	public BaseGame()
	{
		Current = this;
		Event.Register( this );
		Event.RegisterStatics();
		Event.Run( Event.Game.LoadAttribute.Name );
	}

	/// <summary>
	/// This contains a list of user methods that have thrown exceptions, we
	/// don't want to keep calling them if they're problematic so anything in here
	/// should be checked and not called again.
	/// </summary>
	private List<int> FailedMethods { get; } = new();

	private void TryCallMethodOnEntity( string methodName )
	{
		Actor.All.ToList().ForEach( entity =>
		{
			var method = entity.GetType().GetMethod( methodName )!;
			var methodHash = HashCode.Combine( method, entity );

			try
			{
				// Has this method already called an exception?
				// If so, don't call it again!
				if ( FailedMethods.Contains( methodHash ) )
					return;

				method.Invoke( entity, null );
			}
			catch ( Exception ex )
			{
				var targetEx = ex.InnerException ?? ex;

				Notify.AddError( targetEx.GetType().Name, targetEx.Message, FontAwesome.Exclamation );

				Log.Error( targetEx );

				FailedMethods.Add( methodHash );
			}
		} );
	}

	public void FrameUpdate()
	{
		TryCallMethodOnEntity( "FrameUpdate" );
	}

	public void Update()
	{
		if ( Core.IsClient )
		{
			// HACK: Clear DebugOverlay here because doing it
			// per-frame doesn't play nice with tick-based
			// entries (needs fix)

			DebugOverlay.screenTextList.Clear();
			DebugOverlay.currentLine = 0;
		}

		DebugOverlay.ScreenText( $"BaseGame.Update assembly {GetType().Assembly.GetHashCode()}" );

		// Call tick logic on all entities
		TryCallMethodOnEntity( "Update" );

		// Fire tick event
		Event.Run( Event.TickAttribute.Name );
	}

	public void Shutdown()
	{
		OnShutdown();
	}

	public void Startup()
	{
		OnStartup();
	}

	#region "Public API"
	/// <summary>
	/// Called on the server when the game starts up
	/// </summary>
	public virtual void OnStartup()
	{

	}

	/// <summary>
	/// Called on the server when the game shuts down
	/// </summary>
	public virtual void OnShutdown()
	{

	}
	#endregion

	[Event.Game.Hotload]
	public void OnHotload()
	{
		FailedMethods.Clear();
	}
}
