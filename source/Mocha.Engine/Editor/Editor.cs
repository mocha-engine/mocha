namespace Mocha.Engine.Editor;

internal partial class EditorInstance
{
	private const string Font = "qaz";

	private List<Window> Windows = new();

	internal EditorInstance()
	{
		Event.Register( this );
		FontData = FileSystem.Game.Deserialize<Font.Data>( $"core/fonts/baked/{Font}.json" );

		BuildAtlas();
		Graphics.PanelRenderer = new( AtlasTexture );

		var window = new Window();
		window.Bounds = new Rectangle( 32, 32, 500, 750 );
		window.CreateUI();

		Windows.Add( window );
	}

	internal void Render( Veldrid.CommandList commandList )
	{
		Graphics.PanelRenderer.NewFrame();
		Graphics.DrawRect( new Rectangle( 0, (Vector2)Screen.Size ), ITheme.Current.BackgroundColor * 1.25f );

		foreach ( var window in Windows )
			window.Render();

		Graphics.PanelRenderer.Draw( commandList );
	}
}
