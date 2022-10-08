namespace Mocha.Engine.Editor;

internal class BaseLayout
{
	public static List<BaseLayout> All { get; } = new();

	public List<BaseLayout> Layouts { get; } = new();
	private List<(bool Stretch, Widget Widget)> Widgets { get; } = new();
	public bool Visible { get; set; } = true;
	public float Spacing { get; set; } = 0;
	public Vector2 Margin { get; set; } = 0;

	private Vector2 cursor = 0;
	private Vector2 offset = 0;
	private Vector2 calculatedSize;
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

	public BaseLayout()
	{
		All.Add( this );
	}

	public BaseLayout( Vector2 offset ) : this()
	{
		this.offset = offset;
	}

	public void Delete()
	{
		Widgets.ForEach( x => x.Widget.Delete() );
		All.Remove( this );
	}

	public void Add<T>( T widget, bool stretch = false ) where T : Widget
	{
		stretch = true;
		widget.Layout = this;
		Widgets.Add( (stretch, widget) );

		RecalculateLayout();
	}

	private void ApplyStretch()
	{
		for ( int i = 0; i < Widgets.Count; i++ )
		{
			if ( !Widgets[i].Stretch )
				continue;

			var calculatedRect = Widgets[i].Widget.Bounds;
			calculatedRect.Width = CalculatedSize.X - (Margin.X * 2.0f);
			Widgets[i].Widget.Bounds = calculatedRect;
		}
	}

	private void RecalculateLayout()
	{
		cursor = Margin;
		Log.Trace( $"Recalculating layout, cursor is at {cursor}" );

		for ( int i = 0; i < Widgets.Count; i++ )
		{
			var widget = Widgets[i].Widget;
			var desiredSize = widget.GetDesiredSize();
			widget.Bounds = new Rectangle( cursor + offset, desiredSize );

			cursor = cursor.WithY( cursor.Y + desiredSize.Y + Spacing );

			//
			// Update auto-calculated size
			//
			if ( desiredSize.X > calculatedSize.X )
				calculatedSize = calculatedSize.WithX( desiredSize.X );

			if ( cursor.Y > calculatedSize.Y )
				calculatedSize = calculatedSize.WithY( cursor.Y );

			var calculatedRect = Widgets[i].Widget.Bounds;
			calculatedRect.Width = CalculatedSize.X;
			Widgets[i].Widget.Bounds = calculatedRect;
		}

		ApplyStretch();
	}

	internal void AddSpacing( float height )
	{
		//
		// This is kinda hacky.. rather than doing anything complex, we just make
		// a simple dummy widget and let the automatic layout stuff do its work
		//
		var dummy = new Widget() { Bounds = new Rectangle( 0, 0, 0, height ) };
		Add( dummy, true );

		ApplyStretch();
	}

	internal void Render()
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

		foreach ( var layout in Layouts )
		{
			layout.Render();
		}
	}

	public VerticalLayout AddVerticalLayout()
	{
		var verticalLayout = new VerticalLayout();
		Layouts.Add( verticalLayout );

		return verticalLayout;
	}
}
