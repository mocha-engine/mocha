namespace Mocha.Engine.Editor;

internal partial class EditorInstance
{
	internal static EditorInstance Instance { get; private set; }

	private List<Window> Windows = new();

	private bool IsRendering = false;

	internal EditorInstance()
	{
		Event.Register( this );
		Instance = this;

		Graphics.Init();

		Windows.Add( new DemoWindow
		{
			Bounds = new Rectangle( 128, 128, 500, 650 ),
			Focused = true,
			Dock = Dock.Left,
			Title = "Demo Window"
		} );

		Windows.Add( new IconWindow
		{
			Bounds = new Rectangle( 0, 0, 1500, 1080 ),
			Dock = Dock.Bottom,
			Title = "Asset Browser"
		} );

		Windows.Add( new OutlinerWindow
		{
			Bounds = new Rectangle( 0, 0, 1500, 1080 ),
			Dock = Dock.Right,
			Title = "Outliner"
		} );
	}

	internal void Render( Veldrid.CommandList commandList )
	{
		if ( Input.Pressed( InputButton.SwitchMode ) )
			IsRendering = !IsRendering;

		if ( !IsRendering )
			return;

		UpdateWidgets();

		Graphics.PanelRenderer.NewFrame();
		Graphics.DrawRect( new Rectangle( 0, Screen.Size ), MathX.GetColor( "#1e1f21" ) );

		RenderWidgets();

		RenderPerformanceOverlay();

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
			return "Default Light Theme";
		if ( ITheme.Current is DarkTheme )
			return "Default Dark Theme";
		if ( ITheme.Current is TestTheme )
			return "2012";

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

	internal void RenderPerformanceOverlay()
	{
		var framerate = 1.000f / Time.AverageDelta;
		var text = $"FPS: {framerate.CeilToInt()}";

		var size = Graphics.MeasureText( text, 16 ) + 20;
		var position = new Vector2( (Screen.Size.X - size.X) / 2f, 8 );

		var bounds = new Rectangle( position, size );

		Graphics.DrawRect( bounds, ITheme.Current.BackgroundColor, RoundingFlags.All );
		Graphics.DrawText( bounds.Shrink( 8 ), text, 16 );
	}
}
