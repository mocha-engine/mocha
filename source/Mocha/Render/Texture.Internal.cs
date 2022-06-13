namespace Mocha;

/*
 * Generators for internal textures (missing texture etc.)
 */
public partial class TextureBuilder
{
	private static Texture? missingTexture;
	public static Texture MissingTexture
	{
		get
		{
			if ( missingTexture == null )
				CreateMissingTexture();

			return missingTexture;
		}
	}

	[Event.Game.Load]
	public static void CreateMissingTexture()
	{
		//
		// Missing texture
		//
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
				0, 0, 0, 255,		// B
			};

			missingTexture = Texture.Builder
				.FromData( missingTextureData, 4, 4 )
				.WithType( "internal" )
				.WithName( "internal:missing" )
				.Build();
		}
	}
}
