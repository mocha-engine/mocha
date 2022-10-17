namespace Mocha.Engine.Editor;

internal enum Dock
{
	None,
	Left,
	Right,
	Bottom,
	Top
}

internal class Window : Widget
{
	public bool Focused { get; set; }
	protected BaseLayout RootLayout { get; set; }

	private Vector2 lastPos = 0;
	private bool titlebarFocus = false;

	private const int TitlebarHeight = 24;

	public Dock Dock { get; set; }
	public string Title { get; set; } = "Untitled Window";

	//
	// This flag will call CreateUI
	//
	private bool IsDirty = false;

	public Window()
	{
		Event.Register( this );

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

		//
		// Rounding: don't round if docked
		//
		var rounding = (Dock == Dock.None) ? RoundingFlags.All : RoundingFlags.None;

		//
		// Window border
		//
		if ( Dock == Dock.None )
		{
			if ( Focused )
				Graphics.DrawRect( Bounds.Expand( 1 ), Colors.Accent, rounding );
			else
				Graphics.DrawRect( Bounds.Expand( 1 ), Colors.TransparentGray, rounding );
		}
		else
		{
			Graphics.DrawRect( Bounds.Expand( 1 ), ITheme.Current.BackgroundColor, rounding );
		}

		//
		// Main background
		//
		Graphics.DrawRect( Bounds, ITheme.Current.BackgroundColor, rounding );

		if ( Dock == Dock.None )
		{
			//
			// Display titlebar if this is a floating window
			//
			var titlebarBounds = Bounds;
			titlebarBounds.Size = titlebarBounds.Size.WithY( TitlebarHeight );
			Graphics.DrawRect( titlebarBounds, ITheme.Current.ButtonBgA, ITheme.Current.ButtonBgB, RoundingFlags.TopLeft | RoundingFlags.TopRight );
		}
		else
		{
			//
			// Display tab if this is docked
			//
			float dockHeight = 32;
			var colorA = MathX.GetColor( "#33ffffff" );
			var colorB = MathX.GetColor( "#11ffffff" );

			var b = Bounds;
			b.Y += dockHeight + 2;
			b.Height -= dockHeight - 2;
			Graphics.DrawRect( b, colorB );
			var textSize = Graphics.MeasureText( Title );

			var titlebarBounds = Bounds;
			titlebarBounds.Size = titlebarBounds.Size.WithY( dockHeight ).WithX( textSize.X + 24 );
			titlebarBounds.X += 8f;
			titlebarBounds.Y += 2f;
			Graphics.DrawRect( titlebarBounds, colorA, colorB, RoundingFlags.TopLeft | RoundingFlags.TopRight );

			titlebarBounds.X += 12f;
			titlebarBounds.Y += dockHeight / 2f - 8f;
			Graphics.DrawText( titlebarBounds, Title );
		}
	}

	internal override void Update()
	{
		if ( Dock == Dock.None )
		{
			//
			// Floating window
			//
			var titlebarBounds = Bounds;
			titlebarBounds.Size = titlebarBounds.Size.WithY( TitlebarHeight );

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
		}
		else
		{
			//
			// Dock to position
			//
			var b = Bounds;

			if ( Dock == Dock.Left )
			{
				b.X = 0;
				b.Y = 0;
				b.Width = 400;
				b.Height = Screen.Size.Y;
			}

			if ( Dock == Dock.Bottom )
			{
				b.X = 400;
				b.Y = Screen.Size.Y - 400;
				b.Width = Screen.Size.X - 800;
				b.Height = 400;
			}

			if ( Dock == Dock.Right )
			{
				b.X = Screen.Size.X - 400;
				b.Y = 0;
				b.Width = 400;
				b.Height = Screen.Size.Y;
			}

			Bounds = b;
			RootLayout.Bounds = Bounds;
		}

		ZIndex = (Focused) ? 100 : 0;
	}

	[Event.Hotload]
	public void OnHotload()
	{
		IsDirty = true;
	}

	[Event.Window.Resized]
	public void OnWindowResized( Point2 _ )
	{
		IsDirty = true;
	}

	public virtual void CreateUI()
	{

	}
}
