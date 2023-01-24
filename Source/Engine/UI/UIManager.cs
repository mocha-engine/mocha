using Mocha.UI;

namespace Mocha;

public partial class UIManager
{
	public static UIManager Instance { get; private set; }

	private IRenderer Renderer { get; } = new UIEntity();
	private FileSystemWatcher Watcher { get; }
	private bool IsDirty { get; set; }

	public LayoutNode RootPanel { get; private set; }

	private string templatePath;

	public UIManager()
	{
		Event.Register( this );
		Instance = this;
		Graphics.Init();

		Watcher = FileSystem.Game.CreateWatcher( "", "*.*", LoadTemplate );
	}

	public void LoadTemplate( string? file = null )
	{
		bool shouldLoad;

		if ( file == null )
		{
			shouldLoad = true;
		}
		else
		{
			var relativePath = FileSystem.Game.GetRelativePath( file );

			var templatePath = this.templatePath.NormalizePath();
			var stylePath = System.IO.Path.ChangeExtension( this.templatePath.NormalizePath(), "scss" );
			shouldLoad = (relativePath == templatePath || relativePath == stylePath);
		}

		if ( shouldLoad )
		{
			Screen.UpdateFrom( Glue.Editor.GetRenderSize() );
			RootPanel = Template.FromFile( Renderer, templatePath );
			IsDirty = true;
		}
	}

	public void SetTemplate( string path )
	{
		this.templatePath = path;
		LoadTemplate();
	}

	[Event.Window.Resized]
	public void OnResized()
	{
		LoadTemplate();
	}

	public void Render()
	{
		if ( !IsDirty )
			return;

		Graphics.PanelRenderer.NewFrame();

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
