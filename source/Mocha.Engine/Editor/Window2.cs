namespace Mocha.Engine.Editor;

internal class Window2 : Window
{
	public Window2() : base()
	{
	}

	private Image CurrentImage;

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
		RootLayout.Add( new Label( "Images", 64 ) );
		RootLayout.Add( new Label( "Wow!! Pretty pictures!!", 32 ) );
		RootLayout.AddSpacing( 4f );

		//
		// Images test
		//
		var imageDropdown = new Dropdown( "Image Gallery" );

		foreach ( var file in FileSystem.Game.GetFiles( "core/ui" ).Where( x => x.EndsWith( ".mtex_c" ) ) )
		{
			imageDropdown.AddOption( file.NormalizePath() );
		}

		imageDropdown.OnSelected += ( i ) =>
		{
			CurrentImage.SetImage( FileSystem.Game.GetFiles( "core/ui" ).Where( x => x.EndsWith( ".mtex_c" ) ).ToList()[i] );
		};

		RootLayout.Add( imageDropdown );
		CurrentImage = RootLayout.Add( new Image( new Vector2( 300 ), "core/ui/image.mtex" ), true );
	}
}
