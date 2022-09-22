using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Button : Widget
{
	private Label label;
	public Action OnClick;
	public Vector2 TextAnchor = new Vector2( 0.5f, 0.5f );

	public string Text
	{
		get => label.Text;
		set => label.Text = value;
	}

	public Button( string text, Action? onClick = null ) : base()
	{
		label = new( text, 12f );
		label.Parent = this;

		if ( onClick != null )
			OnClick += onClick;
	}

	bool mouseWasDown = false;

	internal override void Render( ref PanelRenderer panelRenderer )
	{
		Vector4 colorA = ITheme.Current.ButtonBgA;
		Vector4 colorB = ITheme.Current.ButtonBgB;

		Vector4 border = ITheme.Current.Border;

		if ( InputFlags.HasFlag( PanelInputFlags.MouseOver ) )
		{
			panelRenderer.AddRectangle( Bounds, Colors.Blue );

			if ( InputFlags.HasFlag( PanelInputFlags.MouseDown ) )
			{
				panelRenderer.AddRectangle( Bounds.Shrink( 1f ),
					colorB * 1.25f,
					colorA * 1.25f,
					colorB * 1.25f,
					colorA * 1.25f
				);

				mouseWasDown = true;
			}
			else
			{
				panelRenderer.AddRectangle( Bounds.Shrink( 1f ),
					colorA,
					colorB,
					colorA,
					colorB
				);

				if ( mouseWasDown )
				{
					OnClick?.Invoke();
				}

				mouseWasDown = false;
			}
		}
		else
		{
			panelRenderer.AddRectangle( Bounds, border );

			panelRenderer.AddRectangle( Bounds.Shrink( 1f ),
				colorA,
				colorB,
				colorA,
				colorB
			);
		}

		var labelBounds = label.Bounds;
		labelBounds.X = Bounds.X + ((Bounds.Width - 24f - Label.MeasureText( label.Text, label.FontSize ).X) * TextAnchor.X);
		labelBounds.X += 12f;
		labelBounds.Y = Bounds.Y + label.FontSize / 3.0f;
		label.Bounds = labelBounds;
	}

	internal override Vector2 GetDesiredSize()
	{
		var size = new Vector2( (Label.MeasureText( label.Text, label.FontSize ).X + 24f).Clamp( 75f, float.MaxValue ), 24 );
		return size;
	}
}
