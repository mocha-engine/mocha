namespace Mocha.UI;

internal class Menu : Widget
{
	protected BaseLayout RootLayout { get; set; }

	//
	// This flag will call CreateUI
	//
	private bool IsDirty = false;

	public Menu()
	{
		Event.Register( this );
		Bounds = new Rectangle( 0, (Vector2)Screen.Size );

		CreateUI();
	}

	internal void Clear()
	{
		RootLayout?.Delete();
		RootLayout = null;
	}

	internal override void Render()
	{
		if ( IsDirty )
		{
			CreateUI();
			IsDirty = false;
		}

		var colorA = Theme.BackgroundColor;
		colorA.W = 0f;
		var colorB = Theme.BackgroundColor;
		colorB.W = 1f;

		Graphics.DrawRect( RootLayout.Bounds, colorA, colorB );
	}

	[Event.Window.Resized]
	public void OnWindowResized()
	{
		CreateUI();
		IsDirty = true;
	}

	public virtual void CreateUI()
	{

	}
}
