namespace Mocha.Engine.Editor;

//
// This is super messy code, just used as a demonstration
// of what custom widgets could potentially achieve in future
//
internal class Icon : Widget
{
	private Texture Texture { get; set; }
	private FileType FileType { get; set; }
	private string FilePath { get; set; }
	private Label Label { get; set; }

	public Icon( string filePath )
	{
		FilePath = filePath;
		var extension = Path.GetExtension( filePath );
		var fileType = FileType.GetFileTypeForExtension( extension ) ?? FileType.Default;

		FileType = fileType;

		if ( fileType.Extension == "mtex" )
		{
			Texture = Texture.Builder.FromPath( filePath ).Build();
		}
		else
		{
			Texture = Texture.Builder.FromPath( FileType.IconLg ).Build();
		}

		Label = new( Path.GetFileName( filePath ) );
		Label.Parent = this;
	}

	float t = 0;

	internal override void Render()
	{
		base.Render();
		var transparentGray = MathX.GetColor( "#77000000" );
		var transparent = MathX.GetColor( "#00000000" );
		var transparentWhite = MathX.GetColor( "#33ffffff" );

		//
		// Background & shadow
		//
		Graphics.DrawShadow( Bounds, 8f, ITheme.Current.ShadowOpacity );
		Graphics.DrawRect( Bounds, FileType.Color, RoundingFlags.All );
		Graphics.DrawRect( Bounds, transparent, transparentGray, RoundingFlags.All );

		//
		// Icon image
		//
		var center = Bounds.Center;
		float imageSize = 96;
		Graphics.DrawTexture( new Rectangle( center - imageSize / 2, imageSize ), Texture );

		//
		// Top/bottom stripes
		//
		var b = Bounds;
		b.Height = 24;
		b.Y = b.Y + Bounds.Height - b.Height;
		Graphics.DrawRect( b, transparentGray, RoundingFlags.BottomLeft | RoundingFlags.BottomRight );

		b.Y = Bounds.Y;
		// Graphics.DrawRect( b, transparentGray, RoundingFlags.TopLeft | RoundingFlags.TopRight );

		//
		// Mouse hover overlay
		//
		t = t.LerpTo( InputFlags.HasFlag( PanelInputFlags.MouseOver ) ? 1 : 0, Time.Delta * 10f );
		var c = transparentWhite;
		c.W *= t;
		Graphics.DrawRect( Bounds, c, RoundingFlags.All );

		//
		// Label
		//
		// TODO: Should probably just have a Graphics.DrawText function
		var lb = Bounds;
		lb.Y = lb.Y + lb.Height - 20;
		lb.X += Bounds.Width / 2f;
		lb.X -= Label.MeasureText( Label.Text, Label.FontSize ).X / 2f;
		Label.Bounds = lb;
	}

	internal override void OnDelete()
	{
		base.OnDelete();

		Label.Delete();
	}

	internal override Vector2 GetDesiredSize()
	{
		float size = 96 * 1.25f;
		return new Vector2( size, size * 1.5f );
	}
}
