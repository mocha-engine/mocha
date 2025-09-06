namespace Mocha;

/*
 * Generators for internal textures (missing texture etc.)
 */
public partial class Texture
{
	private static Texture? s_zero;
	public static Texture Zero => s_zero ?? CreateZeroTexture();

	private static Texture? s_one;
	public static Texture One => s_one ?? CreateOneTexture();

	private static Texture? s_missingTexture;
	public static Texture MissingTexture => s_missingTexture ?? CreateMissingTexture();

	private static Texture? s_normal;
	public static Texture Normal => s_normal ?? CreateNormalTexture();

	public static Texture CreateOneTexture()
	{
		var missingTextureData = new byte[]
		{
			255, 255, 255, 255,
		};

		s_one = new Texture( 1, 1, missingTextureData );
		return s_one;
	}

	public static Texture CreateZeroTexture()
	{
		var missingTextureData = new byte[]
		{
			0, 0, 0, 255,
		};

		s_zero = new Texture( 1, 1, missingTextureData );
		return s_zero;
	}

	// TODO: Change this so that the alpha is zero, and then use the vertex normal
	//		 when alpha is < 255
	public static Texture CreateNormalTexture()
	{
		var normalTextureData = new byte[]
		{
			0, 0, 255, 255
		};

		s_normal = new Texture( 1, 1, normalTextureData );
		return s_normal;
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

		s_missingTexture = new Texture( 2, 2, missingTextureData.ToArray() );
		return s_missingTexture;
	}
}
