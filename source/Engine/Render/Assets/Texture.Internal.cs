namespace Mocha.Renderer;

/*
 * Generators for internal textures (missing texture etc.)
 */
public partial class Texture
{
	private static Texture? zero;
	public static Texture Zero => zero ?? CreateZeroTexture();

	private static Texture? one;
	public static Texture One => one ?? CreateOneTexture();

	private static Texture? missingTexture;
	public static Texture MissingTexture => missingTexture ?? CreateMissingTexture();

	private static Texture? normal;
	public static Texture Normal => normal ?? CreateNormalTexture();

	public static Texture CreateOneTexture()
	{
		var missingTextureData = new byte[]
		{
			255, 255, 255, 255,
		};

		one = new Texture( 1, 1, missingTextureData );
		return one;
	}

	public static Texture CreateZeroTexture()
	{
		var missingTextureData = new byte[]
		{
			0, 0, 0, 255,
		};

		zero = new Texture( 1, 1, missingTextureData );
		return zero;
	}

	// TODO: Change this so that the alpha is zero, and then use the vertex normal
	//		 when alpha is < 255
	public static Texture CreateNormalTexture()
	{
		var normalTextureData = new byte[]
		{
			0, 0, 255, 255
		};

		normal = new Texture( 1, 1, normalTextureData );
		return normal;
	}

	public static Texture CreateMissingTexture()
	{
		var colorA = new byte[] { 200, 200, 200, 255 };
		var colorB = new byte[] { 100, 100, 100, 255 };

		var missingTextureData = new List<byte>();
		missingTextureData.AddRange( colorA );
		missingTextureData.AddRange( colorB );

		missingTextureData.AddRange( colorB );
		missingTextureData.AddRange( colorA );

		missingTexture = new Texture( 2, 2, missingTextureData.ToArray() );
		return missingTexture;
	}
}
