namespace Mocha.Engine.Editor;

internal class Window : Widget
{
	protected VerticalLayout RootLayout { get; set; }
	private const string Lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam sed pharetra lorem. Aliquam eget tristique turpis, eget tristique mi. Nullam et ex vitae mauris dapibus luctus nec vel nisl. Nam venenatis vel orci a sagittis.";
	private bool DrawBounds = false;

	bool titlebarFocus = false;
	Vector2 lastPos = 0;
	bool Focused = false;

	public Window()
	{
		Event.Register( this );
	}

	internal void Clear()
	{
		// Rebuild atlas (TODO: This should be automatic / transparent)
		// BuildAtlas();
		// panelRenderer = new( AtlasTexture );

		RootLayout?.Delete();
		RootLayout = null;
	}

	float t = 0;
	internal override void Render()
	{
		//
		// Main background
		//
		Graphics.DrawShadow( Bounds, 8f, ITheme.Current.ShadowOpacity );
		Graphics.DrawRect( Bounds, ITheme.Current.BackgroundColor );

		//
		// Titlebar
		//
		var titlebarBounds = Bounds;
		titlebarBounds.Size = titlebarBounds.Size.WithY( 32 );
		Graphics.DrawRect( titlebarBounds, ITheme.Current.ButtonBgA, ITheme.Current.ButtonBgB );

		//
		// Window border
		//
		Graphics.DrawRectUnfilled( Bounds, Colors.TransparentGray );

		if ( InputFlags.HasFlag( PanelInputFlags.MouseOver ) )
		{
			t = t.LerpTo( 1.0f, Time.Delta * 15f );
		}
		else
		{
			t = t.LerpTo( 0.0f, Time.Delta * 5f );
		}

		for ( int i = 0; i < 4 * t; ++i )
			Graphics.DrawRectUnfilled( Bounds.Shrink( i ), Colors.Blue * t );

		if ( !InputFlags.HasFlag( PanelInputFlags.MouseDown ) && titlebarFocus )
		{
			titlebarFocus = false;
		}

		if ( titlebarFocus )
		{
			Graphics.DrawRect( titlebarBounds, Colors.Blue );

			var bounds = Bounds;
			bounds.Position += (Vector2)Input.MousePosition - (Vector2)lastPos;
			lastPos = Input.MousePosition;
			Bounds = bounds;

			Widget.All.OfType<Window>().ToList().ForEach( x => x.Focused = false );
			Focused = true;
		}

		if ( InputFlags.HasFlag( PanelInputFlags.MouseDown ) )
		{
			lastPos = Input.MousePosition;
			titlebarFocus = true;
		}

		ZIndex = (Focused) ? 100 : 0;
		RootLayout.Bounds = Bounds;

		//
		// Debug: draw widget bounds
		//
		if ( DrawBounds )
		{
			var widgets = Widget.All.Where( x => x.Visible ).ToList();
			for ( int i = 0; i < widgets.Count; i++ )
			{
				Widget? widget = widgets[i];
				var widgetBounds = widget.Bounds;
				var color = (i % 3) switch
				{
					0 => Colors.Red,
					1 => Colors.Green,
					2 => Colors.Blue,
					_ => Vector4.One
				};

				Graphics.DrawRectUnfilled( widgetBounds, color );
			}
		}
	}

	[Event.Hotload]
	public void OnHotload()
	{
		// CreateUI();
	}

	public virtual void CreateUI()
	{
		using var _ = new Stopwatch( "CreateUI" );

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
		RootLayout.Add( new Label( Lipsum, 13 ) );

		RootLayout.AddSpacing( 4f );

		//
		// Theme switcher (dropdown)
		//
		var themeSwitcher = new Dropdown( EditorInstance.Instance.GetCurrentTheme() );
		themeSwitcher.AddOption( "Dark Theme" );
		themeSwitcher.AddOption( "Light Theme" );
		themeSwitcher.AddOption( "Test Theme" );
		themeSwitcher.OnSelected += EditorInstance.Instance.SwitchTheme;
		RootLayout.Add( themeSwitcher );

		//
		// Debug
		//
		var boundsDropdown = new Dropdown( "Don't Draw Bounds" );
		boundsDropdown.AddOption( "Don't Draw Bounds" );
		boundsDropdown.AddOption( "Draw Bounds" );
		boundsDropdown.OnSelected += ( i ) => DrawBounds = i == 1;
		boundsDropdown.ZIndex = 9;
		RootLayout.Add( boundsDropdown );

		//
		// Different button lengths (sizing test)
		//
		RootLayout.Add( new Button( "Another awesome button" ) );
		RootLayout.Add( new Button( "OK" ) );
		RootLayout.Add( new Button( "I am a really long button with some really long text inside it" ) );
		RootLayout.Add( new Button( "Stretch" ) );

		//
		// Test dropdown
		//
		var dropdown = new Dropdown( "Hello" );
		dropdown.AddOption( "Hello" );
		dropdown.AddOption( "World" );
		dropdown.AddOption( "This is a test" );
		dropdown.AddOption( "I am a really long dropdown entry" );
		dropdown.AddOption( "Poo" );
		RootLayout.Add( dropdown );
	}
}
