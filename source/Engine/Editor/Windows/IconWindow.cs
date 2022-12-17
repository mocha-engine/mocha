namespace Mocha.Engine.Editor;

internal class IconWindow : Window
{
	private void AddFiles( string directory )
	{
		foreach ( var filePath in FileSystem.Game.GetFiles( directory ) )
		{
			var extension = Path.GetExtension( filePath );
			var fileType = FileType.GetFileTypeForExtension( extension );

			if ( !fileType.HasValue )
				continue;

			RootLayout.Add( new Icon( filePath ) );
		}

		foreach ( var subDirectory in FileSystem.Game.GetDirectories( directory ) )
		{
			AddFiles( subDirectory );
		}
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

		RootLayout.Spacing = 12;
		RootLayout.Size = Bounds.Size;
		RootLayout.Margin = new( 16, 48 );

		AddFiles( "." );
	}
}
