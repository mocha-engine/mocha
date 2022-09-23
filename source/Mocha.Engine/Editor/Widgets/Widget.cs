using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Widget
{
	private int zIndex = 0;
	private bool visible = true;

	internal static List<Widget> All { get; } = new();

	public Widget? Parent { get; set; }
	public int ZIndex { get => (Parent?.ZIndex ?? 0) + zIndex; set => zIndex = value; }
	public bool Visible { get => (Parent?.Visible ?? true) && visible; set => visible = value; }

	public PanelInputFlags InputFlags { get; set; }
	public Rectangle Bounds { get; set; }

	internal Widget()
	{
		All.Add( this );
	}

	internal void Delete()
	{
		All.Remove( this );
	}

	internal virtual void Render( ref PanelRenderer panelRenderer )
	{
	}

	internal virtual void OnMouseOver()
	{
	}

	internal virtual void OnMouseDown()
	{
	}

	internal virtual void OnMouseUp()
	{
	}

	internal virtual Vector2 GetDesiredSize()
	{
		return Bounds.Size;
	}
}
