namespace Mocha.Engine.Editor;

internal class Window : Widget
{
	public bool Focused { get; set; }
	protected BaseLayout RootLayout { get; set; }

	private Vector2 lastPos = 0;
	private bool titlebarFocus = false;

	private const int TitlebarHeight = 24;

	//
	// This flag will call CreateUI
	//
	private bool IsDirty = false;

	public Window()
	{
		Event.Register( this );
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

		//
		// Window border
		//
		if ( Focused )
			Graphics.DrawRect( Bounds.Expand( 1 ), Colors.Accent, RoundingFlags.All );
		else
			Graphics.DrawRect( Bounds.Expand( 1 ), Colors.TransparentGray, RoundingFlags.All );

		//
		// Main background
		//
		if ( Focused )
			Graphics.DrawShadow( Bounds, 8f, ITheme.Current.ShadowOpacity * 2f );
		else
			Graphics.DrawShadow( Bounds, 8f, ITheme.Current.ShadowOpacity );

		Graphics.DrawRect( Bounds, ITheme.Current.BackgroundColor, RoundingFlags.All );

		//
		// Titlebar
		//
		var titlebarBounds = Bounds;
		titlebarBounds.Size = titlebarBounds.Size.WithY( TitlebarHeight );
		Graphics.DrawRect( titlebarBounds, ITheme.Current.ButtonBgA, ITheme.Current.ButtonBgB, RoundingFlags.TopLeft | RoundingFlags.TopRight );

		if ( !InputFlags.HasFlag( PanelInputFlags.MouseDown ) && titlebarFocus )
		{
			titlebarFocus = false;
		}

		if ( titlebarFocus )
		{
			var bounds = Bounds;
			bounds.Position += (Vector2)Input.MousePosition - (Vector2)lastPos;
			lastPos = Input.MousePosition;
			Bounds = bounds;

			Widget.All.OfType<Window>().ToList().ForEach( x => x.Focused = false );
			Focused = true;
		}

		if ( InputFlags.HasFlag( PanelInputFlags.MouseDown ) )
		{
			if ( titlebarBounds.Contains( Input.MousePosition ) )
			{
				lastPos = Input.MousePosition;
				titlebarFocus = true;
			}

			if ( new Rectangle( Bounds.X + Bounds.Width - 32, Bounds.Y + Bounds.Height - 32, 32, 32 ).Contains( Input.MousePosition ) )
			{
				Log.Info( "Resize" );
			}
		}

		ZIndex = (Focused) ? 100 : 0;
		RootLayout.Bounds = Bounds;
	}

	[Event.Hotload]
	public void OnHotload()
	{
		IsDirty = true;
	}

	public virtual void CreateUI()
	{

	}
}
