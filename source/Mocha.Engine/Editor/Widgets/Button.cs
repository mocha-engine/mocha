namespace Mocha.Engine.Editor;

internal class Button : Widget
{
	protected Label label;
	public Action OnClick;
	public Vector2 TextAnchor = new Vector2( 0.5f, 0.5f );
	private Vector2 Padding => new Vector2( 20, 15 );

	public string Text
	{
		get => label.Text;
		set => label.Text = value;
	}

	public Button( string text, Action? onClick = null ) : base()
	{
		label = new( text, 13f );
		label.Parent = this;

		if ( onClick != null )
			OnClick += onClick;
	}

	bool mouseWasDown = false;

	internal override void Render()
	{
		Vector4 colorA = ITheme.Current.ButtonBgA;
		Vector4 colorB = ITheme.Current.ButtonBgB;

		Vector4 border = ITheme.Current.Border;

		Graphics.DrawShadow( Bounds, 8f, ITheme.Current.ShadowOpacity );

		if ( InputFlags.HasFlag( PanelInputFlags.MouseOver ) )
		{
			Graphics.DrawRect( Bounds, Colors.Blue );

			if ( InputFlags.HasFlag( PanelInputFlags.MouseDown ) )
			{
				Graphics.DrawRect( Bounds.Shrink( 1f ), colorB, colorA );

				mouseWasDown = true;
			}
			else
			{
				Graphics.DrawRect( Bounds.Shrink( 1f ), colorA, colorB );

				if ( mouseWasDown )
				{
					OnClick?.Invoke();
				}

				mouseWasDown = false;
			}
		}
		else
		{
			Graphics.DrawRect( Bounds, border );
			Graphics.DrawRect( Bounds.Shrink( 1f ), colorA, colorB );
		}

		UpdateLabel();
	}

	protected void UpdateLabel()
	{
		var labelBounds = label.Bounds;
		labelBounds.X = Bounds.X + ((Bounds.Width - (Padding.X * 2.0f) - Label.MeasureText( label.Text, label.FontSize ).X) * TextAnchor.X);
		labelBounds.X += Padding.X;
		labelBounds.Y = Bounds.Y + (Padding.Y) - 8;
		label.Bounds = labelBounds;
	}

	internal override Vector2 GetDesiredSize()
	{
		var size = new Vector2( (Label.MeasureText( label.Text, label.FontSize ).X + (Padding.X * 2)).Clamp( 75f, float.MaxValue ), Padding.Y * 2 );
		return size;
	}
}
