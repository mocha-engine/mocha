namespace Mocha.Engine.Editor;

internal class Icon : Widget
{
	private Image Image { get; set; }
	private Texture Texture { get; set; }
	private FileType FileType { get; set; }

	public Icon( FileType fileType )
	{
		FileType = fileType;
		Texture = Texture.Builder.FromPath( FileType.IconLg ).Build();
	}

	float t = 0;

	internal override void Render()
	{
		base.Render();

		Graphics.DrawShadow( Bounds, 8f, ITheme.Current.ShadowOpacity );
		Graphics.DrawRect( Bounds, FileType.Color, RoundingFlags.All );
		Graphics.DrawRect( Bounds, MathX.GetColor( "#00000000" ), MathX.GetColor( "#77000000" ), RoundingFlags.All );

		var center = Bounds.Center;
		float imageSize = 96;
		Graphics.DrawTexture( new Rectangle( center - imageSize / 2, imageSize ), Texture );

		var b = Bounds;
		b.Height = 8;
		b.Y = b.Y + Bounds.Height - b.Height;
		Graphics.DrawRect( b, MathX.GetColor( "#77000000" ), RoundingFlags.BottomLeft | RoundingFlags.BottomRight );

		b.Y = Bounds.Y;
		Graphics.DrawRect( b, MathX.GetColor( "#77000000" ), RoundingFlags.TopLeft | RoundingFlags.TopRight );

		if ( InputFlags.HasFlag( PanelInputFlags.MouseOver ) )
		{
			t = t.LerpTo( 1.0f, Time.Delta * 10f );
		}
		else
		{
			t = t.LerpTo( 0.0f, Time.Delta * 10f );
		}

		var c = MathX.GetColor( "#88ffffff" );
		c.W *= t;
		Graphics.DrawRect( Bounds, c, RoundingFlags.All );
	}

	internal override Vector2 GetDesiredSize()
	{
		float size = 96 * 1.25f;
		return new Vector2( size, size * 1.5f );
	}
}
