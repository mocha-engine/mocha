namespace Mocha.Engine.Editor;

internal class OutlinerWindow : Menu
{
	public override void CreateUI()
	{
		Clear();

		RootLayout = new VerticalLayout
		{
			Size = (Vector2)Screen.Size,
			Parent = this
		};

		RootLayout.Spacing = 10;
		RootLayout.Size = Bounds.Size;
		RootLayout.Margin = new( 16, 48 );

		for ( int i = 0; i < 8; ++i )
		{
			RootLayout.Add( new Button( $"Entity {i}" ) );
		}
	}
}
