namespace Mocha.UI;
internal class MainMenu : Menu
{
	public override void CreateUI()
	{
		//
		// Clean up existing widgets & panels
		//
		Clear();

		//
		// Everything has to go inside a layout otherwise they'll go in funky places
		//
		Bounds = new Rectangle( Vector2.Zero, new Vector2( 500f, Screen.Size.Y ) );
		RootLayout = new VerticalLayout
		{
			Bounds = Bounds,
			Size = new Vector2( 500f, Screen.Size.Y ),
			Parent = this
		};

		RootLayout.Spacing = 10;
		RootLayout.Margin = new( 16, Screen.Size.Y / 2f - 180 );

		RootLayout.Add( new Label( $"{FontAwesome.ShuttleSpace} SpaceGame", 64 ) );
		RootLayout.Add( new Label( $"{FontAwesome.Vial} {Glue.Editor.GetVersionName()}", 16 ) );

		RootLayout.AddSpacing( 16f );

		RootLayout.Add( new Button( "Singleplayer", () =>
		{
			Delete();
			UIManager.SetSubMenu( null );
		} ), true );

		RootLayout.Add( new Button( "Server Browser", () => UIManager.SetSubMenu( new ServerBrowserMenu() ) ), true );
		RootLayout.Add( new Button( "Settings", () => UIManager.SetSubMenu( new SettingsMenu() ) ), true );
		RootLayout.Add( new Button( "Quit", () => Environment.Exit( 0 ) ), true );

		RootLayout.AddSpacing( Screen.Size.Y / 2f - 200f );

		RootLayout.Add( new Label( $"{FontAwesome.Copyright} 2022 Alex Guthrie" ) );
	}
}
