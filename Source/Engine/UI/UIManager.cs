namespace Mocha.UI;

internal partial class UIManager
{
	public bool Debug { get; set; } = false;

	internal static UIManager Instance { get; private set; }

	private Texture Crosshair { get; } = new Texture( "ui/crosshair.mtex", false );

	private IRenderer Renderer { get; } = new UIEntity();
	public LayoutNode RootPanel { get; private set; }

	private const string Path = "ui/Game.html";

	private FileSystemWatcher Watcher { get; }

	private bool IsDirty { get; set; }

	internal UIManager()
	{
		Event.Register( this );
		Instance = this;
		Graphics.Init();

		var directory = System.IO.Path.GetDirectoryName( Path );
		Watcher = FileSystem.Game.CreateWatcher( directory, "*.*", _ => LoadTemplate() );

		LoadTemplate();
	}

	[Event.Window.Resized]
	public void LoadTemplate()
	{
		Screen.UpdateFrom( Glue.Editor.GetRenderSize() );
		RootPanel = Template.FromFile( Renderer, Path );
		IsDirty = true;
	}

	internal void Render()
	{
		if ( !IsDirty )
			return;

		Graphics.PanelRenderer.NewFrame();
		Graphics.DrawTexture( new Rectangle( (Vector2)Screen.Size / 2f - 16f, new Vector2( 32, 32 ) ), Crosshair );

		DrawNode( RootPanel );
		IsDirty = false;
	}

	internal void DrawNode( LayoutNode layoutNode )
	{
		if ( layoutNode.StyledNode.Node is ElementNode elementNode )
		{
			var backgroundColor = layoutNode.StyledNode.StyleValues.BackgroundColor ?? ColorValue.Transparent;
			var rounding = layoutNode.StyledNode.StyleValues.BorderRadius?.Value ?? 0;
			var bounds = layoutNode.Bounds;

			Renderer.DrawRectangle( bounds, backgroundColor, rounding );

			var backgroundImage = layoutNode.StyledNode.StyleValues.BackgroundImage?.Value;
			if ( backgroundImage != null )
			{
				Renderer.DrawImage( bounds, backgroundImage );
			}
		}
		else if ( layoutNode.StyledNode.Node is TextNode textNode )
		{
			var color = layoutNode.StyledNode.Parent.StyleValues.Color ?? ColorValue.White;
			var fontSize = layoutNode.StyledNode.Parent.StyleValues.FontSize?.Value ?? 16;
			var weight = layoutNode.StyledNode.Parent.StyleValues.FontWeight?.Value ?? 400;
			var font = layoutNode.StyledNode.Parent.StyleValues.FontFamily?.Value ?? "Inter";

			var bounds = layoutNode.Bounds;

			Renderer.DrawText( bounds, textNode.Text, font, (int)weight, fontSize, color );
		}

		foreach ( var childNode in layoutNode.Children )
			DrawNode( childNode );
	}
}
