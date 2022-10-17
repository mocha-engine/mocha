namespace Mocha.Engine.Editor;

//
// This is super messy code, just used as a demonstration
// of what custom widgets could potentially achieve in future
//
internal class Icon : Widget
{
	private Texture Texture { get; set; }
	private FileType FileType { get; set; }
	private string FileName { get; set; }
	private float Size => 144;

	public Icon( string filePath )
	{
		var extension = Path.GetExtension( filePath );
		var fileType = FileType.GetFileTypeForExtension( extension ) ?? FileType.Default;

		FileName = Path.GetFileName( filePath );
		FileType = fileType;

		if ( fileType.Extension == "mtex" )
		{
			Texture = Texture.Builder.FromPath( filePath ).Build();
		}
		else if ( fileType.Extension == "mmat" )
		{
			var material = Material.FromPath( filePath );
			Texture = material.DiffuseTexture;
		}
		else
		{
			Texture = Texture.Builder.FromPath( FileType.IconLg ).Build();
		}
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
		float imageSize = Size / 1.5f;
		Graphics.DrawTexture( new Rectangle( center - imageSize / 2, imageSize ), Texture );

		//
		// Mouse hover overlay
		//
		t = t.LerpTo( InputFlags.HasFlag( PanelInputFlags.MouseOver ) ? 1 : 0, Time.Delta * 10f );
		var c = transparentWhite;
		c.W *= t;
		Graphics.DrawRect( Bounds, c, RoundingFlags.All );

		//
		// Top/bottom stripes
		//
		var b = Bounds;
		b.Height = 32;
		b.Y = b.Y + Bounds.Height - b.Height;
		Graphics.DrawRect( b, transparentGray, RoundingFlags.BottomLeft | RoundingFlags.BottomRight );

		b.Y = Bounds.Y;
		Graphics.DrawRect( b, transparentGray, RoundingFlags.TopLeft | RoundingFlags.TopRight );

		//
		// Text
		//
		var lb = Bounds;
		lb.Y = lb.Y + lb.Height - 24;
		lb.X += Bounds.Width / 2f;
		lb.X -= Graphics.MeasureText( FileName ).X / 2f;
		Graphics.DrawText( lb, FileName );

		lb.Y = Bounds.Y + 8;
		lb.X = Bounds.X + Bounds.Width - 24;
		Graphics.DrawText( lb, FileType.IconSm );

		{
			lb.X -= 18f;
			Graphics.DrawText( lb, FontAwesome.HardDrive );
		}

		if ( FileName.Contains( "subaru" ) )
		{
			lb.X -= 18f;
			Graphics.DrawText( lb, FontAwesome.Star, Colors.Yellow );
		}

		if ( FileName.Contains( "_c" ) )
		{
			lb.X -= 18f;
			Graphics.DrawText( lb, FontAwesome.Check, Colors.Green );
		}
	}

	internal override Vector2 GetDesiredSize()
	{
		return new Vector2( Size, Size * 1.25f );
	}
}
