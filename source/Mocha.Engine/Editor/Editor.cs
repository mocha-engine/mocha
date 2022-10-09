namespace Mocha.Engine.Editor;

internal partial class EditorInstance
{
	internal static EditorInstance Instance { get; private set; }

	private const string Font = "qaz";

	private List<Window> Windows = new();

	internal EditorInstance()
	{
		Event.Register( this );
		Instance = this;
		FontData = FileSystem.Game.Deserialize<Font.Data>( $"core/fonts/baked/{Font}.json" );

		InitializeAtlas();
		Graphics.PanelRenderer = new( AtlasTexture );

		var window = new Window();
		window.Bounds = new Rectangle( 32, 32, 500, 600 );
		window.CreateUI();
		Windows.Add( window );

		var window2 = new Window2();
		window2.Bounds = new Rectangle( 128, 32, 500, 600 );
		window2.CreateUI();
		Windows.Add( window2 );
	}

	internal void Render( Veldrid.CommandList commandList )
	{
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
			widget.Render();
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
