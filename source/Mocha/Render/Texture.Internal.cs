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
				0, 255, 0, 255,
				255, 0, 255, 255,
				0, 255, 0, 255,
				255, 0, 255, 255,
			};

			missingTexture = Texture.Builder
				.FromData( missingTextureData, 2, 2 )
				.WithType( "internal" )
				.WithName( "internal:missing" )
				.Build();
		}
	}
}
