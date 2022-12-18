namespace Mocha.UI;
internal class ServerBrowserMenu : SubMenu
{
	struct ServerBrowserEntry
	{
		public ServerBrowserEntry( string name, int playerCount, int maxPlayers, int ping )
		{
			Name = name;
			PlayerCount = playerCount;
			MaxPlayers = maxPlayers;
			Ping = ping;
		}

		public string Name { get; set; }
		public int PlayerCount { get; set; }
		public int MaxPlayers { get; set; }
		public int Ping { get; set; }
	}

	private ServerBrowserEntry[] ServerBrowserEntries => new[]
	{
		new ServerBrowserEntry( "Fake Server 1", 4, 32, 8 ),
		new ServerBrowserEntry( "Fake Server 2", 6, 64, 10 ),
		new ServerBrowserEntry( "Fake Server 3", 12, 24, 5 ),
		new ServerBrowserEntry( "Fake Server 4", 8, 16, 12 ),
		new ServerBrowserEntry( "Fake Server 5", 16, 32, 7 )
	};

	public override void CreateUI()
	{
		base.CreateUI();

		RootLayout.Spacing = 10;
		RootLayout.Margin = new( 16, 32 );

		RootLayout.Add( new Label( $"{FontAwesome.List} Server Browser", 32 ) );

		foreach ( var entry in ServerBrowserEntries )
		{
			RootLayout.Add( new Label( $"{FontAwesome.Server} {entry.Name}" ) );
			RootLayout.Add( new Label( $"{FontAwesome.User} {entry.PlayerCount} / {entry.MaxPlayers}" ) );
			RootLayout.Add( new Label( $"{FontAwesome.Wifi} {entry.Ping}" ) );
			RootLayout.Add( new Button( "Connect" ) );

			RootLayout.AddSpacing( 16f );
		}
	}
}
