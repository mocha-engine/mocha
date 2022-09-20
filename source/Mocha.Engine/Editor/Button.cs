using Mocha.Renderer.UI;

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
		label = new( text, rect );
	}

	internal override void Render( ref PanelRenderer panelRenderer )
	{
		Vector4 colorA = Colors.Gray * 0.75f;
		Vector4 colorB = Colors.DarkGray * 0.75f;

		if ( rect.Contains( Input.MousePosition ) )
		{
			panelRenderer.AddRectangle( rect.Expand( 1f ), Colors.Blue );

			if ( Input.MouseLeft )
			{
				panelRenderer.AddRectangle( rect,
					colorB * 1.25f,
					colorA * 1.25f,
					colorB * 1.25f,
					colorA * 1.25f
				);
			}
			else
			{
				panelRenderer.AddRectangle( rect,
					colorA,
					colorB,
					colorA,
					colorB
				);
			}
		}
		else
		{
			panelRenderer.AddRectangle( rect.Expand( 1f ), Colors.Black );

			panelRenderer.AddRectangle( rect,
				colorA,
				colorB,
				colorA,
				colorB
			);
		}

		label.Render( ref panelRenderer );
	}
}
