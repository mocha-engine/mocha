namespace Mocha.Engine.Editor;

internal class IconWindow : Window
{
	public IconWindow() : base()
	{
	}

	public override void CreateUI()
	{
		//
		// Clean up existing widgets & panels
		//
		Clear();

		//
		// Everything has to go inside a layout otherwise they'll go in funky places
		//
		RootLayout = new GridLayout
		{
			Size = (Vector2)Screen.Size,
			Parent = this
		};

		RootLayout.Spacing = 10;
		RootLayout.Size = Bounds.Size;
		RootLayout.Margin = new( 16, 48 );

		foreach ( var fileType in FileType.All )
		{
			RootLayout.Add( new Icon( fileType ), false );
		}
	}
}
