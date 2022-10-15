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
		Graphics.DrawRect( Bounds, FileType.Color, true );
		Graphics.DrawRect( Bounds, MathX.GetColor( "#00000000" ), MathX.GetColor( "#77000000" ), true );
		Graphics.DrawTexture( new Rectangle( Bounds.X, Bounds.Y + (Bounds.Height - Bounds.Width) / 2.0f, Bounds.Width, Bounds.Width ), Texture );
		// Graphics.DrawRectUnfilled( Bounds, MathX.GetColor( "#77000000" ), 2.0f );

		if ( InputFlags.HasFlag( PanelInputFlags.MouseOver ) )
		{
			Graphics.DrawRect( Bounds, MathX.GetColor( "#88ffffff" ), true );
		}
	}

	internal override Vector2 GetDesiredSize()
	{
		return new Vector2( 96, 96 * 1.5f );
	}
}
