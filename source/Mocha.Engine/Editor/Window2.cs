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
		RootLayout.Add( new Label( "Icons", 64 ) );
		RootLayout.Add( new Label( "Different file types and stuff!", 32 ) );
		RootLayout.AddSpacing( 4f );

		//
		// Images test
		//
		foreach ( var file in FileSystem.Game.GetFiles( "core/ui/icons" ).Where( x => x.EndsWith( ".mtex_c" ) ) )
		{
			Log.Trace( $"Adding {file}" );
			RootLayout.Add( new Image( new Vector2( 96 ), file ), true );
		}
	}
}
