namespace Mocha.Renderer;

/*
 * Generators for internal textures (missing texture etc.)
 */
public partial class TextureBuilder
{
	private static Texture? zero;
	public static Texture Zero => zero ?? CreateZeroTexture();

	private static Texture? one;
	public static Texture One => one ?? CreateOneTexture();

	private static Texture? missingTexture;
	public static Texture MissingTexture => missingTexture ?? CreateMissingTexture();

	public static Texture CreateOneTexture()
	{

		var missingTextureData = new byte[]
		{
			255, 255, 255, 255,
		};

		one = new TextureBuilder()
			.FromData( missingTextureData, 1, 1 )
			.WithName( "internal:one" )
			.Build();


		return one;
	}


	public static Texture CreateZeroTexture()
	{

		var missingTextureData = new byte[]
		{
			0, 0, 0, 255,
		};

		zero = new TextureBuilder()
			.FromData( missingTextureData, 1, 1 )
			.WithName( "internal:zero" )
			.Build();


		return zero;
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

		missingTexture = new TextureBuilder()
			.FromData( missingTextureData, 4, 4 )
			.WithName( "internal:missing" )
			.Build();


		return missingTexture;
	}
}
