namespace Mocha.UI;

internal class SubMenu : Menu
{
	public override void CreateUI()
	{
		base.CreateUI();

		//
		// Clean up existing widgets & panels
		//
		Clear();

		//
		// Everything has to go inside a layout otherwise they'll go in funky places
		//
		Bounds = new Rectangle( new Vector2( 516f, 0 ), new Vector2( Screen.Size.X - 516f, Screen.Size.Y ) );
		RootLayout = new VerticalLayout
		{
			Bounds = Bounds,
			Size = new Vector2( 500f, Screen.Size.Y ),
			Parent = this
		};
	}
}
