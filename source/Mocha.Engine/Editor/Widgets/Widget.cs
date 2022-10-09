namespace Mocha.Engine.Editor;

internal class Widget
{
	private int zIndex = 0;
	private bool visible = true;

	internal static List<Widget> All { get; } = new();

	public BaseLayout Layout { get; set; }
	public Widget? Parent { get; set; }
	public int ZIndex { get => (Parent?.ZIndex ?? 0) + zIndex; set => zIndex = value; }
	public bool Visible { get => (Parent?.Visible ?? true) && visible; set => visible = value; }

	public PanelInputFlags InputFlags { get; set; }

	public Rectangle RelativeBounds { get; private set; }
	public Rectangle Bounds
	{
		get => RelativeBounds + (Layout?.Bounds.Position ?? 0);
		set
		{
			RelativeBounds = value;
			OnBoundsChanged();
		}
	}

	internal Widget()
	{
		All.Add( this );
	}

	internal void Delete()
	{
		Layout?.Remove( this );
		All?.Remove( this );
	}

	internal virtual void Render()
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

	internal virtual void OnBoundsChanged()
	{

	}
}
