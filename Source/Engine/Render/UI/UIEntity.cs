using Mocha.UI;

namespace Mocha;

[Icon( FontAwesome.Square ), Title( "UI" )]
public partial class UIEntity : ModelEntity, IRenderer
{
	public AtlasBuilder AtlasBuilder { get; set; }
	private Material Material { get; set; }
	private UIModel Model { get; set; }

	private bool IsDirty { get; set; }

	public UIEntity()
	{
		IsUI = true;
		AtlasBuilder = new();
		Material = new( "shaders/ui/ui.mshdr",
				 UIVertex.VertexAttributes,
				 AtlasBuilder.Texture,
				 sampler: SamplerType.Anisotropic,
				 ignoreDepth: true );
	}

	public void NewFrame()
	{
		_vertices.Clear();
		_rectCount = 0;
	}

	public void AddRectangle( Common.Rectangle rect,
		Common.Rectangle ndcTexRect,
		Vector4 colorA,
		Vector4 colorB,
		Vector4 colorC,
		Vector4 colorD,
		GraphicsFlags flags,
		float rounding )
	{
		if ( rect.X > Screen.Size.X || rect.Y > Screen.Size.Y )
			return;

		var ndcRect = rect / (Vector2)Screen.Size;
		var vertices = RectVertices.Select( ( x, i ) =>
		{
			var position = x.Position;
			position.X = (x.Position.X * ndcRect.Size.X) + ndcRect.Position.X;
			position.Y = (x.Position.Y * ndcRect.Size.Y) + ndcRect.Position.Y;

			var texCoords = x.TexCoords;
			texCoords.X = (x.TexCoords.X * ndcTexRect.Size.X) + ndcTexRect.Position.X;
			texCoords.Y = (x.TexCoords.Y * ndcTexRect.Size.Y) + ndcTexRect.Position.Y;

			var tx = x;
			position *= 2.0f;
			position.X -= 1.0f;
			position.Y -= 1.0f;

			// Pack the bytes!
			// Lower 2 bytes of Flags is the flags
			// Upper 2 bytes of Flags is the rounding
			var flagsInt = (int)flags;
			flagsInt |= (int)rounding << 16;

			tx.Flags = flagsInt;
			tx.Position = position;
			tx.TexCoords = texCoords;
			tx.PanelPos *= rect.Size;
			tx.PanelSize = rect.Size;
			tx.Color = i switch
			{
				0 => colorA,
				1 => colorB,
				2 => colorC,
				3 => colorD,
				_ => Vector4.Zero,
			};

			return tx;
		} ).ToArray();

		_vertices.AddRange( vertices );
		_rectCount++;

		IsDirty = true;
	}
	private static string GetFont( string fontFamily, int weight )
	{
		var fontName = weight switch
		{
			100 => $"{fontFamily}-Thin",
			200 => $"{fontFamily}-ExtraLight",
			300 => $"{fontFamily}-Light",
			400 => $"{fontFamily}-Regular",
			500 => $"{fontFamily}-Medium",
			600 => $"{fontFamily}-SemiBold",
			700 => $"{fontFamily}-Bold",
			800 => $"{fontFamily}-ExtraBold",
			_ => $"{fontFamily}-Regular",
		};

		if ( FileSystem.Mounted.Exists( $"fonts/{fontName}.mfnt" ) )
			return fontName;

		if ( FileSystem.Mounted.Exists( $"fonts/{fontFamily}.mfnt" ) )
			return fontFamily;

		return "Inter-Regular";
	}

	public void DrawRectangle( Rectangle bounds, ColorValue color, float rounding = 0 )
	{
		bounds.Size *= Screen.UIScale;
		bounds.Position *= Screen.UIScale;
		rounding *= Screen.UIScale;

		Graphics.DrawRect( bounds, color.ToVector4(), RoundingFlags.All, rounding );
	}

	public void DrawText( Rectangle bounds, string text, string fontFamily, int weight, float fontSize, ColorValue color )
	{
		bounds.Size *= Screen.UIScale;
		bounds.Position *= Screen.UIScale;
		fontSize *= Screen.UIScale;

		Graphics.DrawText( bounds, text, GetFont( fontFamily, weight ), fontSize, color.ToVector4() );
	}

	public Vector2 CalcTextSize( string text, string fontFamily, int weight, float fontSize )
	{
		return Graphics.MeasureText( text, GetFont( fontFamily, weight ), fontSize );
	}

	public void DrawImage( Rectangle bounds, string path )
	{
		bounds.Size *= Screen.UIScale;
		bounds.Position *= Screen.UIScale;

		// Replace .png and .jpg in case I forget to change it to mtex myself..
		path = path.Replace( ".jpg", ".mtex" ).Replace( ".png", ".mtex" );
		Graphics.DrawTexture( bounds, path );
	}
}
