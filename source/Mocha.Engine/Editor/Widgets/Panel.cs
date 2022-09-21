﻿using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Panel : Widget
{
	public Vector4 Color { get; set; } = new Vector4( 1, 1, 1, 1 );

	internal Panel( Vector2 size )
	{
		Bounds = new Rectangle( 0, size );
	}

	internal virtual void Render( ref PanelRenderer panelRenderer )
	{
		panelRenderer.AddRectangle( Bounds, Color );
	}
}
