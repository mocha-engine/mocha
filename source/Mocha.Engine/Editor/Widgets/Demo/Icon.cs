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

	internal override void Render()
	{
		base.Render();

		Graphics.DrawShadow( Bounds, 8f, ITheme.Current.ShadowOpacity );
		Graphics.DrawRect( Bounds, FileType.Color, Renderer.UI.RoundingFlags.All );
		Graphics.DrawRect( Bounds, MathX.GetColor( "#00000000" ), MathX.GetColor( "#77000000" ), Renderer.UI.RoundingFlags.All );

		var center = Bounds.Center;
		float imageSize = 96;
		Graphics.DrawTexture( new Rectangle( center - imageSize / 2, imageSize ), Texture );

		var b = Bounds;
		b.Height = 8;
		b.Y = b.Y + Bounds.Height - b.Height;
		Graphics.DrawRect( b, MathX.GetColor( "#77000000" ), Renderer.UI.RoundingFlags.BottomLeft | Renderer.UI.RoundingFlags.BottomRight );

		b.Y = Bounds.Y;
		Graphics.DrawRect( b, MathX.GetColor( "#77000000" ), Renderer.UI.RoundingFlags.TopLeft | Renderer.UI.RoundingFlags.TopRight );

		if ( InputFlags.HasFlag( PanelInputFlags.MouseOver ) )
		{
			Graphics.DrawRect( Bounds, MathX.GetColor( "#88ffffff" ), Renderer.UI.RoundingFlags.All );
		}
	}

	internal override Vector2 GetDesiredSize()
	{
		float size = 96 * 1.25f;
		return new Vector2( size, size * 1.5f );
	}
}
