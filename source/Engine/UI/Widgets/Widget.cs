namespace Mocha.UI;

internal class Widget
{
	private int zIndex = 0;
	private bool visible = true;

	internal static List<Widget> All { get; } = new();

	public BaseLayout Layout { get; set; }
	public Widget? Parent { get; set; }
	public List<Widget> Children => All.Where( x => x.Parent == this ).ToList();
	public List<BaseLayout> Layouts => BaseLayout.All.Where( x => x.Parent == this ).ToList();

	public int ZIndex { get => (Parent?.ZIndex ?? 0) + zIndex; set => zIndex = value; }
	public bool Visible { get => (Parent?.Visible ?? true) && visible; set => visible = value; }
	public bool IsDeleted { get; set; }

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

	internal virtual void OnDelete()
	{

	}

	internal void Delete()
	{
		OnDelete();

		IsDeleted = true;
		Layout?.Remove( this );
		All?.Remove( this );
		Event.Unregister( this );

		DeleteChildren();
	}

	internal void DeleteChildren()
	{
		Children.ForEach( x => x.Delete() );
		Layouts.ForEach( x => x.Delete() );
	}

	internal virtual void Render()
	{

	}

	internal virtual void Update()
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
