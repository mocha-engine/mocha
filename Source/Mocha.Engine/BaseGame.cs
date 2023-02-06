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
		BaseEntity.All.ToList().ForEach( entity =>
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
				Notify.AddError( ex.GetType().Name, ex.Message, FontAwesome.Exclamation );

				Log.Error( ex );

				FailedMethods.Add( methodHash );
			}
		} );
	}

	public virtual void FrameUpdate()
	{
		UIManager.Instance.Render();

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

		Event.Run( Event.TickAttribute.Name );
	}

	public virtual void Shutdown()
	{
	}

	public virtual void Startup()
	{
	}

	[Event.Game.Hotload]
	public void OnHotload()
	{
		FailedMethods.Clear();
	}
}
