namespace Mocha.Engine.Editor;

internal class Window2 : Window
{
	public Window2() : base()
	{
	}

	public override void CreateUI()
	{
		using var _ = new Stopwatch( "CreateUI" );

		//
		// Clean up existing widgets & panels
		//
		Clear();

		//
		// Everything has to go inside a layout otherwise they'll go in funky places
		//
		RootLayout = new VerticalLayout
		{
			Size = (Vector2)Screen.Size,
			Parent = this
		};

		RootLayout.Spacing = 10;
		RootLayout.Size = Bounds.Size;
		RootLayout.Margin = new( 16, 32 );

		//
		// Text rendering
		//
		var title = RootLayout.Add( new Label( "Icons", 64 ) );
		title.SetFont( "wavetosh" );

		var subtitle = RootLayout.Add( new Label( "Different file types and stuff!", 32 ) );
		subtitle.SetFont( "inter" );

		RootLayout.AddSpacing( 4f );

		foreach ( var fileType in FileType.All )
		{
			RootLayout.Add( new Icon( fileType ), false );
		}
	}
}
