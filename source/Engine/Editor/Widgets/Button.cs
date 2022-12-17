namespace Mocha.Engine.Editor;

internal class Button : Widget
{
	public Action OnClick;
	public Vector2 TextAnchor = new Vector2( 0.5f, 0.5f );
	private Vector2 Padding => new Vector2( 20, 15 );

	public string Text { get; set; } = "";

	public Button( string text, Action? onClick = null ) : base()
	{
		if ( onClick != null )
			OnClick += onClick;

		Text = text;
	}

	bool mouseWasDown = false;

	internal override void Render()
	{
		Vector4 colorA = ITheme.Current.ButtonBgA;
		Vector4 colorB = ITheme.Current.ButtonBgB;

		Vector4 border = ITheme.Current.Border;

		Graphics.DrawShadow( Bounds, 2f, ITheme.Current.ShadowOpacity );
		Graphics.DrawRect( Bounds, border, RoundingFlags.All );

		if ( InputFlags.HasFlag( PanelInputFlags.MouseDown ) )
		{
			mouseWasDown = true;
			Graphics.DrawRect( Bounds.Shrink( 1f ), colorB, colorA, RoundingFlags.All );
		}
		else
		{
			var b = Bounds.Shrink( 1f );
			Graphics.DrawRect( b, ITheme.Current.ButtonBgA * 1.25f, RoundingFlags.All );
			float d = 1f;
			b.Height -= d;
			b.Y += d;

			if ( InputFlags.HasFlag( PanelInputFlags.MouseOver ) )
			{
				Graphics.DrawRect( b, colorA * 0.75f, colorA * 0.75f, RoundingFlags.All );
			}
			else
			{
				Graphics.DrawRect( b, colorA, colorB, RoundingFlags.All );
			}
			if ( mouseWasDown )
			{
				OnClick?.Invoke();
			}

			mouseWasDown = false;
		}

		UpdateLabel();
	}

	protected void UpdateLabel()
	{
		var labelBounds = Bounds;
		labelBounds.X = Bounds.X + ((Bounds.Width - (Padding.X * 2.0f) - Graphics.MeasureText( Text ).X) * TextAnchor.X);
		labelBounds.X += Padding.X;
		labelBounds.Y = Bounds.Y + (Padding.Y) - 8;

		Graphics.DrawText( labelBounds, Text );
	}

	internal override Vector2 GetDesiredSize()
	{
		var size = new Vector2( (Graphics.MeasureText( Text ).X + (Padding.X * 2)).Clamp( 75f, float.MaxValue ), Padding.Y * 2 );
		return size;
	}
}
