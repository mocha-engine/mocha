using Mocha.Networking;

namespace Mocha;

public class BaseGame : IGame
{
	public static BaseGame Current { get; set; }

	private static Server? s_server;
	private static Client? s_client;

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

	public void FrameUpdate()
	{
		UIManager.Instance.Render();

		TryCallMethodOnEntity( "FrameUpdate" );
	}

	public void Update()
	{
		// TODO: This is garbage and should not be here!!!
		s_server?.Update();
		s_client?.Update();

		if ( Core.IsClient )
		{
			// HACK: Clear DebugOverlay here because doing it
			// per-frame doesn't play nice with tick-based
			// entries (needs fix)

			DebugOverlay.screenTextList.Clear();
			DebugOverlay.currentLine = 0;
		}
	}

	public void Shutdown()
	{
		OnShutdown();
	}

	public void Startup()
	{
		if ( Core.IsClient )
		{
			s_client = new BaseGameClient( "127.0.0.1" );
		}
		else
		{
			s_server = new BaseGameServer()
			{
				OnClientConnectedEvent = ( connection ) => OnClientConnected( connection.GetClient() ),
				OnClientDisconnectedEvent = ( connection ) => OnClientDisconnected( connection.GetClient() ),
			};
		}

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

	/// <summary>
	/// Called on the server whenever a client joins
	/// </summary>
	public virtual void OnClientConnected( IClient client )
	{

	}

	/// <summary>
	/// Called on the server whenever a client leaves
	/// </summary>
	public virtual void OnClientDisconnected( IClient client )
	{

	}
	#endregion

	[Event.Game.Hotload]
	public void OnHotload()
	{
		FailedMethods.Clear();
	}
}
