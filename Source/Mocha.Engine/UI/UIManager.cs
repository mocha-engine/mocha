using Mocha.UI;

namespace Mocha;

public partial class UIManager
{
	public static UIManager Instance { get; private set; }
	public LayoutNode RootPanel { get; private set; }

	private string _templatePath;
	private bool _isDirty;
	private readonly List<FileSystemWatcher> _watchers;
	private readonly IRenderer _renderer = new UIEntity();

	public UIManager()
	{
		Instance = this;

		Event.Register( this );
		Graphics.Init();

		_watchers = FileSystem.Mounted.CreateMountedFileSystemWatchers( "*.*", LoadTemplate );
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
			var relativePath = FileSystem.Mounted.GetRelativePath( file );

			var templatePath = _templatePath.NormalizePath();
			var stylePath = Path.ChangeExtension( _templatePath.NormalizePath(), "scss" );
			shouldLoad = (relativePath == templatePath || relativePath == stylePath);
		}

		if ( shouldLoad )
		{
			Screen.UpdateFrom( NativeEngine.GetRenderSize() );
			RootPanel = Template.FromFile( _renderer, _templatePath );
			_isDirty = true;
		}
	}

	public void SetTemplate( string path )
	{
		this._templatePath = path;
		LoadTemplate();
	}

	[Event.Window.Resized]
	public void OnResized()
	{
		LoadTemplate();
	}

	public void Render()
	{
		if ( !_isDirty )
			return;

		Graphics.PanelRenderer.NewFrame();

		DrawNode( RootPanel );
		_isDirty = false;
	}

	internal void DrawNode( LayoutNode layoutNode )
	{
		if ( layoutNode.StyledNode.Node is ElementNode elementNode )
		{
			var backgroundColor = layoutNode.StyledNode.StyleValues.BackgroundColor ?? ColorValue.Transparent;
			var rounding = layoutNode.StyledNode.StyleValues.BorderRadius?.Value ?? 0;
			var bounds = layoutNode.Bounds;

			_renderer.DrawRectangle( bounds, backgroundColor, rounding );

			var backgroundImage = layoutNode.StyledNode.StyleValues.BackgroundImage?.Value;
			if ( backgroundImage != null )
			{
				_renderer.DrawImage( bounds, backgroundImage );
			}
		}
		else if ( layoutNode.StyledNode.Node is TextNode textNode )
		{
			var color = layoutNode.StyledNode.Parent.StyleValues.Color ?? ColorValue.White;
			var fontSize = layoutNode.StyledNode.Parent.StyleValues.FontSize?.Value ?? 16;
			var weight = layoutNode.StyledNode.Parent.StyleValues.FontWeight?.Value ?? 400;
			var font = layoutNode.StyledNode.Parent.StyleValues.FontFamily?.Value ?? "Inter";

			var bounds = layoutNode.Bounds;

			_renderer.DrawText( bounds, textNode.Text, font, (int)weight, fontSize, color );
		}

		foreach ( var childNode in layoutNode.Children )
			DrawNode( childNode );
	}
}
