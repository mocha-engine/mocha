namespace Mocha.Engine.Editor;
internal class DemoWindow : Window
{
	private const string Lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam sed pharetra lorem. Aliquam eget tristique turpis, eget tristique mi. Nullam et ex vitae mauris dapibus luctus nec vel nisl. Nam venenatis vel orci a sagittis.";

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
			Size = (Vector2)Screen.Size,
			Parent = this
		};

		RootLayout.Spacing = 10;
		RootLayout.Size = Bounds.Size;
		RootLayout.Margin = new( 16, 32 );

		//
		// Text rendering
		//
		RootLayout.Add( new Label( "Test Window", 64 ) );
		RootLayout.Add( new Label( "Lots of different widgets", 32 ) );
		RootLayout.Add( new Label( $"{FontAwesome.FaceSmile} Icons!! {FontAwesome.FaceGrin} {FontAwesome.Icons}", 16 ) );
		RootLayout.Add( new Label( Lipsum ) );
		RootLayout.Add( new Label( Lipsum, 32 ) );

		// Different font families
		RootLayout.Add( new Label( "Source Sans Pro: The quick brown fox jumps over the lazy dog", fontFamily: "sourcesanspro" ) );
		RootLayout.Add( new Label( "Inter: The quick brown fox jumps over the lazy dog", fontFamily: "inter" ) );
		RootLayout.Add( new Label( "Qaz: The quick brown fox jumps over the lazy dog", fontFamily: "qaz" ) );
		RootLayout.Add( new Label( "Wavetosh: The quick brown fox jumps over the lazy dog", fontFamily: "wavetosh" ) );

		RootLayout.AddSpacing( 4f );

		//
		// Theme switcher (dropdown)
		//
		var themeSwitcher = new Dropdown( EditorInstance.Instance.GetCurrentTheme() );
		themeSwitcher.AddOption( "Default Dark Theme" );
		themeSwitcher.AddOption( "Default Light Theme" );
		themeSwitcher.AddOption( "2012" );
		themeSwitcher.OnSelected += EditorInstance.Instance.SwitchTheme;
		RootLayout.Add( themeSwitcher );

		//
		// Debug enabled (dropdown)
		//
		var debugToggle = new Dropdown( EditorInstance.Instance.GetCurrentDebug() );
		debugToggle.AddOption( "Debug Disabled" );
		debugToggle.AddOption( "Debug Enabled" );
		debugToggle.OnSelected += EditorInstance.Instance.SwitchDebug;
		RootLayout.Add( debugToggle );

		//
		// Different button lengths (sizing test)
		//
		RootLayout.Add( new Button( "OK" ) );
		RootLayout.Add( new Button( "I am a really long button with some really long text inside it" ) );

		//
		// Test dropdown
		//
		var dropdown = new Dropdown( "Hello" );
		dropdown.AddOption( "Hello" );
		dropdown.AddOption( "World" );
		dropdown.AddOption( "This is a test" );
		dropdown.AddOption( "I am a really long dropdown entry" );
		dropdown.AddOption( $"{FontAwesome.Poo} Poo" );
		RootLayout.Add( dropdown );
	}
}
