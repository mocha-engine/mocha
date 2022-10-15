namespace Mocha.Engine.Editor;

internal class VerticalLayout : BaseLayout
{
	public override void CalculateWidgetBounds( Widget widget, bool stretch )
	{
		var desiredSize = widget.GetDesiredSize();
		widget.Bounds = new Rectangle( cursor + Margin, desiredSize );

		cursor = cursor.WithY( cursor.Y + desiredSize.Y + Spacing );

		//
		// Update auto-calculated size
		//
		if ( desiredSize.X > calculatedSize.X )
			calculatedSize = calculatedSize.WithX( desiredSize.X );

		if ( cursor.Y > calculatedSize.Y )
			calculatedSize = calculatedSize.WithY( cursor.Y );

		if ( stretch )
		{
			var calculatedRect = widget.RelativeBounds;
			calculatedRect.Width = CalculatedSize.X - (Margin.X * 2.0f);
			widget.Bounds = calculatedRect;
		}
	}
}

// TODO
internal class HorizontalLayout : BaseLayout
{
}

internal class GridLayout : BaseLayout
{
	public override void CalculateWidgetBounds( Widget widget, bool stretch )
	{
		var desiredSize = widget.GetDesiredSize();

		float maxWidth = desiredSize.X, maxHeight = desiredSize.Y;

		if ( Widgets.Any() )
		{
			maxHeight = Widgets.Select( x => x.Widget.Bounds.Height ).Max();
			maxWidth = Widgets.Select( x => x.Widget.Bounds.Width ).Max();
		}

		widget.Bounds = new Rectangle( cursor + Margin, desiredSize );

		cursor = cursor.WithX( cursor.X + maxWidth + Spacing );

		if ( cursor.X + maxWidth > Size.X )
			cursor = cursor.WithY( cursor.Y + maxHeight + Spacing ).WithX( 0 );

		//
		// Update auto-calculated size
		//
		if ( desiredSize.X > calculatedSize.X )
			calculatedSize = calculatedSize.WithX( desiredSize.X );

		if ( cursor.Y > calculatedSize.Y )
			calculatedSize = calculatedSize.WithY( cursor.Y );
	}
}
