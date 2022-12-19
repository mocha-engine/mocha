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

	class ServerBrowserRow : Button
	{
		public ServerBrowserRow( ServerBrowserEntry entry ) : base( "" )
		{
			Text = $"{entry.Name,-64} {$"{entry.PlayerCount}/{entry.MaxPlayers}",-8} {$"{entry.Ping}ms",-8}";
		}
	}

	class Table : Widget
	{
		private List<ServerBrowserEntry> Items { get; } = new();

		class TableRow : Widget
		{
			public bool IsHeader { get; set; }
			public Table Table { get; set; }
			public List<string> Values { get; set; }

			bool mouseWasDown = false;
			public Action OnClick;

			float Padding = 8f;

			internal override void Render()
			{
				Vector4 colorA = new( 0, 0, 0, 0.1f );
				Vector4 colorB = new( 0, 0, 0, 0.2f );

				Vector4 border = Theme.Border;

				if ( InputFlags.HasFlag( PanelInputFlags.MouseOver ) )
				{
					if ( InputFlags.HasFlag( PanelInputFlags.MouseDown ) )
					{
						Graphics.DrawRect( Bounds,
							colorB * 1.25f,
							colorA * 1.25f,
							colorB * 1.25f,
							colorA * 1.25f
						);

						mouseWasDown = true;
					}
					else
					{
						Graphics.DrawRect( Bounds, Theme.Accent );
						Graphics.DrawRect( Bounds,
							colorA * 0.5f,
							colorB * 0.5f,
							colorA * 0.5f,
							colorB * 0.5f
						);

						if ( mouseWasDown )
						{
							OnClick?.Invoke();
						}

						mouseWasDown = false;
					}
				}

				var cursorPos = Bounds.Position + new Vector2( 0, Padding - 2f );
				for ( int i = 0; i < Values.Count; ++i )
				{
					var value = Values[i];
					cursorPos.X = Bounds.X + Table.GetColumnX( i ) + 8;

					var bounds = new Rectangle( cursorPos, 512 );

					if ( IsHeader )
						Graphics.DrawText( bounds, value, 16 );
					else
						Graphics.DrawText( bounds, value );
				}
			}

			internal override Vector2 GetDesiredSize()
			{
				var textSize = Graphics.MeasureText( "W" ); // We just need height of 1 character
				return new Vector2( -1, textSize.Y + Padding * 2f );
			}
		}

		private List<TableRow> options = new();

		public void AddItem( ServerBrowserEntry item )
		{
			var row = new TableRow();

			row.Values = new List<string>()
			{
				item.Name,
				$"{item.PlayerCount}/{item.MaxPlayers}",
				$"{item.Ping}ms"
			};

			row.Parent = this;
			row.Table = this;

			options.Add( row );
		}

		public void SetHeader( params string[] headerValues )
		{
			var row = new TableRow();
			row.Values = headerValues.ToList();

			row.IsHeader = true;
			row.Parent = this;
			row.Table = this;

			options.Insert( 0, row );
		}

		private int GetColumnX( int index )
		{
			if ( index == 0 )
				return 0;
			else if ( index == 1 )
				return GetColumnX( 0 ) + (Bounds.Width - 175).CeilToInt();
			else if ( index == 2 )
				return GetColumnX( 1 ) + 125;

			return 0;
		}

		internal override void Render()
		{
			base.Render();

			var cursor = Bounds.Position + new Vector2( 0, GetDesiredSize().Y );
			foreach ( var option in options )
			{
				var desiredSize = option.GetDesiredSize();
				if ( desiredSize.X > Bounds.Width )
				{
					var newBounds = Bounds;
					newBounds.Width = desiredSize.X + 64f;
					Bounds = newBounds;

					Log.Trace( $"Dropdown entry was bigger than dropdown width, resizing dropdown to {desiredSize.X}" );
				}

				desiredSize.X = Bounds.Width;

				option.Bounds = new Rectangle( cursor, desiredSize );

				cursor += new Vector2( 0, desiredSize.Y );

				if ( option.IsHeader )
					cursor += new Vector2( 0, 8 );
			}
		}

		internal override Vector2 GetDesiredSize()
		{
			return new Vector2( 0, 1 );
		}
	}

	private List<ServerBrowserEntry> ServerBrowserEntries => new()
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

		var table = RootLayout.Add( new Table(), true );
		table.SetHeader( "Server Name", "Player Count", "Ping" );
		ServerBrowserEntries.ForEach( table.AddItem );
	}
}
