namespace Mocha.Engine.Editor;

internal class VerticalLayout
{
	public static List<VerticalLayout> All { get; } = new();

	private List<(bool Stretch, Widget Widget)> Widgets { get; } = new();
	public bool Visible { get; set; } = true;
	public float Spacing { get; set; } = 0;
	public Vector2 Margin
	{
		get => cursor;
		set => cursor = value;
	}

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

	public VerticalLayout()
	{
		All.Add( this );
	}

	public VerticalLayout( Vector2 offset ) : this()
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
		var desiredSize = widget.GetDesiredSize();
		widget.Bounds = new Rectangle( cursor + offset, desiredSize );
		Widgets.Add( (stretch, widget) );

		cursor = cursor.WithY( cursor.Y + desiredSize.Y + Spacing );

		//
		// Update auto-calculated size
		//
		if ( desiredSize.X > calculatedSize.X )
			calculatedSize = calculatedSize.WithX( desiredSize.X );

		if ( cursor.Y > calculatedSize.Y )
			calculatedSize = calculatedSize.WithY( cursor.Y );

		ApplyStretch();
	}

	private void ApplyStretch()
	{
		for ( int i = 0; i < Widgets.Count; i++ )
		{
			if ( !Widgets[i].Stretch )
				continue;

			var calculatedRect = Widgets[i].Widget.Bounds;
			calculatedRect.Width = CalculatedSize.X;
			Widgets[i].Widget.Bounds = calculatedRect;
		}
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
}
