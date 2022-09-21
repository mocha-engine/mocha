using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Button : Panel
{
	private Label label;
	public Action onClick;

	public string Text
	{
		get => label.Text;
		set => label.Text = value;
	}

	public Button( string text, Rectangle bounds ) : base( bounds )
	{
		label = new( text, bounds, 12f );
	}

	bool mouseWasDown = false;

	internal override void Render( ref PanelRenderer panelRenderer )
	{
		Vector4 colorA = ITheme.Current.ButtonBgA;
		Vector4 colorB = ITheme.Current.ButtonBgB;

		Vector4 border = ITheme.Current.Border;

		if ( Bounds.Contains( Input.MousePosition ) )
		{
			panelRenderer.AddRectangle( Bounds.Expand( 1f ), Colors.Blue );

			if ( Input.MouseLeft )
			{
				panelRenderer.AddRectangle( Bounds,
					colorB * 1.25f,
					colorA * 1.25f,
					colorB * 1.25f,
					colorA * 1.25f
				);

				mouseWasDown = true;
			}
			else
			{
				panelRenderer.AddRectangle( Bounds,
					colorA,
					colorB,
					colorA,
					colorB
				);

				if ( mouseWasDown )
				{
					onClick?.Invoke();
				}

				mouseWasDown = false;
			}
		}
		else
		{
			panelRenderer.AddRectangle( Bounds.Expand( 1f ), border );

			panelRenderer.AddRectangle( Bounds,
				colorA,
				colorB,
				colorA,
				colorB
			);
		}

		label.Bounds.X = Bounds.X + ((Bounds.Width - Label.MeasureText( label.Text, label.FontSize ).X) / 2.0f);
		label.Bounds.Y = Bounds.Y + label.FontSize / 3.0f;

		Bounds.Width = (Label.MeasureText( label.Text, label.FontSize ).X + 25f).Clamp( 75f, float.MaxValue );
		label.Render( ref panelRenderer );
	}
}
