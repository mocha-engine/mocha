namespace Mocha.Engine.Editor;

internal partial class EditorInstance
{
	internal static EditorInstance Instance { get; private set; }

	private List<Window> Windows = new();

	internal EditorInstance()
	{
		Event.Register( this );
		Instance = this;

		Graphics.Init();

		var demoWindow = new DemoWindow();
		demoWindow.Bounds = new Rectangle( 128, 128, 500, 650 );
		demoWindow.Focused = true;
		demoWindow.CreateUI();
		Windows.Add( demoWindow );

		var iconWindow = new IconWindow();
		iconWindow.Bounds = new Rectangle( 0, 0, 1500, 1080 );
		iconWindow.CreateUI();
		Windows.Add( iconWindow );
	}

	internal void Render( Veldrid.CommandList commandList )
	{
		UpdateWidgets();

		Graphics.PanelRenderer.NewFrame();
		Graphics.DrawRect( new Rectangle( 0, (Vector2)Screen.Size ), ITheme.Current.BackgroundColor * 1.25f );

		RenderWidgets();

		Graphics.PanelRenderer.Draw( commandList );
	}

	internal void RenderWidgets()
	{
		var widgets = Widget.All.Where( x => x.Visible ).OrderBy( x => x.ZIndex ).ToList();
		var mouseOverWidgets = widgets.Where( x => x.Bounds.Contains( Input.MousePosition ) );

		foreach ( var widget in widgets )
		{
			widget.Render();
		}
	}

	internal void UpdateWidgets()
	{
		var widgets = Widget.All.Where( x => x.Visible ).OrderBy( x => x.ZIndex ).ToList();
		var mouseOverWidgets = widgets.Where( x => x.Bounds.Contains( Input.MousePosition ) );

		foreach ( var widget in widgets )
		{
			widget.InputFlags = PanelInputFlags.None;
		}

		if ( mouseOverWidgets.Any() )
		{
			var focusedWidget = mouseOverWidgets.Last();
			focusedWidget.InputFlags |= PanelInputFlags.MouseOver;

			if ( Input.MouseLeft )
			{
				focusedWidget.InputFlags |= PanelInputFlags.MouseDown;
			}
		}

		foreach ( var widget in widgets )
		{
			widget.Update();
		}
	}

	internal string GetCurrentTheme()
	{
		if ( ITheme.Current is LightTheme )
			return "Light Theme";
		if ( ITheme.Current is DarkTheme )
			return "Dark Theme";
		if ( ITheme.Current is TestTheme )
			return "Test Theme";

		return "???";
	}

	internal void SwitchTheme( int newSelection )
	{
		Log.Trace( newSelection );

		if ( newSelection == 0 )
			ITheme.Current = new DarkTheme();
		else if ( newSelection == 1 )
			ITheme.Current = new LightTheme();
		else
			ITheme.Current = new TestTheme();

		Renderer.Window.Current.SetDarkMode( ITheme.Current is not LightTheme );

		Windows.ForEach( x => x.CreateUI() );
	}
}
