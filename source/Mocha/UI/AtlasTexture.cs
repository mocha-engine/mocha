namespace Mocha.Renderer.UI;

public class Sprite
{
	public AtlasBuilder Parent { get; private set; }
	public Common.Rectangle Rect { get; private set; }

	internal Vector4[] TextureData { get; private set; }

	internal Sprite( Common.Rectangle rect, AtlasBuilder parent )
	{
		Rect = rect;
		Parent = parent;
	}

	public void SetData( Vector4[] data )
	{
		TextureData = data;
	}
}

public class AtlasBuilder
{
	private List<Sprite> sprites = new();

	public AtlasBuilder()
	{
	}

	private Point2 CalculateSize()
	{
		//
		// Get total texture size
		//
		int width = 0;
		int height = 0;

		foreach ( var sprite in sprites )
		{
			var max = sprite.Rect.Position + sprite.Rect.Size;

			if ( max.X > width )
				width = (int)max.X;

			if ( max.Y > height )
				height = (int)max.Y;
		}

		return new Point2( width, height );
	}

	internal void Update( Common.Rectangle bounds, Vector4[] data )
	{
		if ( data.Length != bounds.Size.Length )
		{
			throw new Exception( "Fuck you" );
		}
	}

	public Sprite AddSprite( Point2 size )
	{
		var position = new Vector2( 0, 0 );

		//
		// TODO: pack these properly
		// and do it all ahead-of-time..
		//

		if ( sprites.Any() )
		{
			var lastSpriteRect = sprites.Last().Rect;
			var (width, height) = CalculateSize();

			position = lastSpriteRect.Position + new Vector2( lastSpriteRect.Size.X, 0 );
			position.Y = 0;
		}

		var sprite = new Sprite( new Common.Rectangle( position, (Vector2)size ), this );
		sprites.Add( sprite );

		return sprite;
	}

	public void RemoveSprite( Sprite sprite )
	{
		Log.Trace( $"Freeing sprite with size {sprite.Rect.Size}" );
		sprites.Remove( sprite );
	}

	public Texture Build()
	{
		var (width, height) = CalculateSize();

		Log.Trace( $"Building atlas with size {(width, height)}" );

		//
		// Collect all texture data
		//
		Vector4[] data = new Vector4[width * height];
		for ( int x = 0; x < width; x++ )
		{
			for ( int y = 0; y < height; y++ )
			{
				foreach ( var sprite in sprites )
				{
					if ( sprite.TextureData == null )
						continue;

					if ( sprite.Rect.Contains( new Vector2( x, y ) ) )
					{
						var relativeCoords = new Vector2( x, y ) - sprite.Rect.Position;
						data[x + (y * width)] = sprite.TextureData[(int)relativeCoords.X + (int)(relativeCoords.Y * sprite.Rect.Width)];

						break;
					}
				}
			}
		}

		//
		// Turn Vec4[] into byte[]
		//
		var TextureData = new byte[width * height * 4];
		int i = 0;
		foreach ( var pixel in data )
		{
			TextureData[i] = (byte)(pixel.X * 255);
			TextureData[i + 1] = (byte)(pixel.Y * 255);
			TextureData[i + 2] = (byte)(pixel.Z * 255);
			TextureData[i + 3] = (byte)(pixel.W * 255);

			i += 4;
		}

		//
		// Make texture
		//
		var texture = TextureBuilder.UITexture.FromData( TextureData, (uint)width, (uint)height ).IgnoreCache().Build();
		return texture;
	}
}
