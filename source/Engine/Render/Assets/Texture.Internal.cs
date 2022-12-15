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
		var missingTextureData = new byte[]
		{
			//
			0, 0, 0, 255,		// B
			255, 0, 255, 255,	// P
			0, 0, 0, 255,		// B
			255, 0, 255, 255,	// P

			//
			255, 0, 255, 255,	// P
			0, 0, 0, 255,		// B
			255, 0, 255, 255,	// P
			0, 0, 0, 255,		// B

			//
			0, 0, 0, 255,		// B
			255, 0, 255, 255,	// P
			0, 0, 0, 255,		// B
			255, 0, 255, 255,	// P

			//
			255, 0, 255, 255,	// P
			0, 0, 0, 255,		// B
			255, 0, 255, 255,	// P
			0, 0, 0, 255,       // B
		};

		missingTexture = new Texture( 4, 4, missingTextureData );
		return missingTexture;
	}
}
