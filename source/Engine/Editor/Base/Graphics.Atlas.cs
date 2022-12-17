using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;
partial class Graphics
{
	internal static Texture UITexture { get; set; }
	private static List<(Texture Texture, Vector2 Position)> TextureCache { get; } = new();

	public static void Init()
	{
		PanelRenderer = new();
		InitializeAtlas();
	}

	private static void InitializeAtlas()
	{
		//
		// White box
		//
		var whiteSpriteData = new byte[32 * 32 * 4];
		Array.Fill( whiteSpriteData, (byte)255 );
		UITexture = new Texture( 32, 32, whiteSpriteData );

		PanelRenderer.AtlasBuilder.AddOrGetTexture( UITexture );
	}
}
