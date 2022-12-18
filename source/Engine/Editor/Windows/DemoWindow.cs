namespace Mocha.Engine.Editor;
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
		RootLayout = new VerticalLayout
		{
			Bounds = new Rectangle( Vector2.Zero, new Vector2( 500f, Screen.Size.Y ) ),
			Size = new Vector2( 500f, Screen.Size.Y ),
			Parent = this
		};

		RootLayout.Spacing = 10;
		RootLayout.Margin = new( 16, Screen.Size.Y / 2f - 180 );

		RootLayout.Add( new Label( $"{FontAwesome.ShuttleSpace} SpaceGame", 64 ) );
		RootLayout.Add( new Label( $"{FontAwesome.Vial} {Glue.Editor.GetVersionName()}", 16 ) );

		RootLayout.AddSpacing( 16f );

		RootLayout.Add( new Button( "Singleplayer" ), true );
		RootLayout.Add( new Button( "Server Browser" ), true );
		RootLayout.Add( new Button( "Options" ), true );
		RootLayout.Add( new Button( "Quit", () => Environment.Exit( 0 ) ), true );

		RootLayout.AddSpacing( Screen.Size.Y / 2f - 180f );

		RootLayout.Add( new Label( $"{FontAwesome.Copyright} 2022 Alex Guthrie", 12 ) );
	}

	internal override void Render()
	{
		base.Render();

		var colorA = new Vector4( 0, 0, 0, 0f );
		var colorB = new Vector4( 0, 0, 0, 1.0f );

		Graphics.DrawRect( RootLayout.Bounds, colorA, colorB );
	}
}
