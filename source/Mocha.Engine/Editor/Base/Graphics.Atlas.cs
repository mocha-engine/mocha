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
		UITexture = new TextureBuilder().FromData( whiteSpriteData, 32, 32 ).WithName( "internal:editor_white_box" ).Build();

		PanelRenderer.AtlasBuilder.AddOrGetTexture( UITexture );
	}
}
