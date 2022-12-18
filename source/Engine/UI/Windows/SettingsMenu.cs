namespace Mocha.UI;
internal class SettingsMenu : SubMenu
{
	public override void CreateUI()
	{
		base.CreateUI();

		RootLayout.Spacing = 10;
		RootLayout.Margin = new( 16, Screen.Size.Y / 2f - 300 );

		RootLayout.Add( new Label( $"{FontAwesome.Gear} Settings", 32 ) );

		//
		// Display
		//
		RootLayout.AddSpacing( 16f );
		RootLayout.Add( new Label( $"{FontAwesome.Display} Display", 16 ) );

		RootLayout.Add( new Label( $"Display Mode" ) );
		var displayModeDropdown = RootLayout.Add( new Dropdown( "Windowed" ), true );
		displayModeDropdown.AddOption( "Windowed" );
		displayModeDropdown.AddOption( "Fullscreen" );
		displayModeDropdown.AddOption( "Borderless Fullscreen" );

		RootLayout.Add( new Label( $"Resolution" ) );
		var resolutionDropdown = RootLayout.Add( new Dropdown( $"{Screen.Size.X}x{Screen.Size.Y}" ), true );
		resolutionDropdown.AddOption( "1280x720" );
		resolutionDropdown.AddOption( "1920x1080" );
		resolutionDropdown.AddOption( "3840x2160" );

		//
		// Camera
		//
		RootLayout.AddSpacing( 16f );
		RootLayout.Add( new Label( $"{FontAwesome.Camera} Camera", 16 ) );

		RootLayout.Add( new Label( $"Field of View" ) );
		RootLayout.Add( new Button( "90" ), true );

		//
		// Audio
		//
		RootLayout.AddSpacing( 16f );
		RootLayout.Add( new Label( $"{FontAwesome.VolumeHigh} Audio", 16 ) );

		RootLayout.Add( new Label( $"Master Volume" ) );
		RootLayout.Add( new Button( "100" ), true );

		RootLayout.AddSpacing( Screen.Size.Y / 2f - 230 );

		RootLayout.Add( new Button( $"{FontAwesome.FloppyDisk} Save" ), true );
	}
}
