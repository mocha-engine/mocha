using Mocha.Renderer.UI;
using System.Diagnostics.Metrics;

namespace Mocha.Engine;

internal class Button : Panel
{
	private Label label;

	public string Text
	{
		get => label.Text;
		set => label.Text = value;
	}

	public Button( string text, Rectangle rect ) : base( rect )
	{
		label = new( text, rect, 12f );
	}

	internal override void Render( ref PanelRenderer panelRenderer )
	{
		Vector4 colorA = Colors.Gray * 0.75f;
		Vector4 colorB = Colors.DarkGray * 0.75f;

		if ( rect.Contains( Input.MousePosition ) )
		{
			panelRenderer.AddRoundedRectangle( rect.Expand( 1f ),
				4f,
				Colors.Blue
			);

			if ( Input.MouseLeft )
			{
				panelRenderer.AddRoundedRectangle( rect,
					4f,
					colorB * 1.25f
				);
			}
			else
			{
				panelRenderer.AddRoundedRectangle( rect,
					4f,
					colorB
				);
			}
		}
		else
		{
			panelRenderer.AddRoundedRectangle( rect.Expand( 1f ), 4f, colorB );

			panelRenderer.AddRoundedRectangle( rect,
				4f,
				colorA
			);
		}

		label.rect.X = rect.X + ((rect.Width - Label.MeasureText( label.Text, label.FontSize ).X) / 2.0f);
		label.rect.Y = rect.Y + label.FontSize / 3.0f;

		rect.Width = ( Label.MeasureText( label.Text, label.FontSize ).X + 25f ).Clamp( 75f, float.MaxValue );
		label.Render( ref panelRenderer );
	}
}
