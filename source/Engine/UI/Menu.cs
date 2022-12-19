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
		DeleteChildren();
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

		var color = Theme.BackgroundColor;
		color.W = 0.75f;

		Graphics.DrawRect( RootLayout.Bounds, color );
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
