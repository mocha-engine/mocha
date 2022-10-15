namespace Mocha.Engine.Editor;

internal class BaseLayout
{
	public static List<BaseLayout> All { get; } = new();

	public List<BaseLayout> Layouts { get; } = new();
	protected List<(bool Stretch, Widget Widget)> Widgets { get; } = new();
	public bool Visible { get; set; } = true;
	public float Spacing { get; set; } = 0;
	public Vector2 Margin { get; set; } = 0;

	protected Vector2 cursor = 0;
	protected Vector2 calculatedSize;

	public Vector2 Size { get; set; } = new( -1, -1 );
	public Vector2 CalculatedSize
	{
		get
		{
			if ( Size.Length > 0 )
				return Size;

			return calculatedSize;
		}
	}

	public Rectangle Bounds { get; set; }
	public Widget? Parent { get; set; }

	public BaseLayout()
	{
		All.Add( this );
	}

	public void Delete()
	{
		Widgets.ToList().ForEach( x => x.Widget.Delete() );
		All.Remove( this );
	}

	public virtual void CalculateWidgetBounds( Widget widget, bool stretch )
	{

	}

	public T Add<T>( T widget, bool stretch = true ) where T : Widget
	{
		widget.Layout = this;

		if ( Parent != null )
			widget.Parent = Parent;

		CalculateWidgetBounds( widget, stretch );

		Widgets.Add( (stretch, widget) );

		return widget;
	}

	internal void AddSpacing( float height )
	{
		//
		// This is kinda hacky.. rather than doing anything complex, we just make
		// a simple dummy widget and let the automatic layout stuff do its work
		//
		var dummy = new Widget() { Bounds = new Rectangle( 0, 0, 0, height ) };
		Add( dummy, true );
	}

	public T AddLayout<T>() where T : BaseLayout
	{
		var layout = Activator.CreateInstance<T>();
		layout.Parent = Parent;
		layout.Margin = cursor;

		Layouts.Add( layout );
		return layout;
	}

	public void Remove( Widget widget )
	{
		Widgets.RemoveAll( x => x.Widget == widget );
	}
}
