using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class VerticalLayout
{
	public static List<VerticalLayout> All { get; } = new();

	private List<( bool Stretch, Widget Widget )> Widgets { get; } = new();
	public bool Visible { get; set; } = true;
	public float Spacing { get; set; } = 0;
	public Vector2 Margin
	{
		get => cursor;
		set => cursor = value;
	}

	private Vector2 cursor = 0;
	private Vector2 offset = 0;
	public Vector2 CalculatedSize { get; private set; }

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
		All.Remove( this );
	}

	public void AddWidget<T>( T widget, bool stretch = false ) where T : Widget
	{
		var desiredSize = widget.GetDesiredSize();
		widget.Bounds = new Rectangle( cursor + offset, desiredSize );
		Widgets.Add( ( stretch, widget ) );

		cursor = cursor.WithY( cursor.Y + desiredSize.Y + Spacing );

		if ( desiredSize.X > CalculatedSize.X )
			CalculatedSize = CalculatedSize.WithX( desiredSize.X );

		if ( cursor.Y > CalculatedSize.Y )
			CalculatedSize = CalculatedSize.WithY( cursor.Y );

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

	public void Render( PanelRenderer panelRenderer )
	{
		if ( !Visible )
			return;

		Widgets.OrderBy( x => x.Widget.ZIndex ).ToList().ForEach( x => x.Widget.Render( ref panelRenderer ) );
	}

	internal void AddSpacing( float height )
	{
		Widgets.Add( ( true, new Widget() { Bounds = new Rectangle( 0, 0, 0, height ) } ) );

		ApplyStretch();
	}
}
