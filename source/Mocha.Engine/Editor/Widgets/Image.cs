using Mocha.Common.Serialization;
using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Image : Widget
{
	public Vector4 Color { get; set; } = new Vector4( 1, 1, 1, 1 );
	public TextureInfo TextureInfo { get; set; }
	internal static Sprite? TestSprite { get; set; }

	internal Image( Vector2 size, string path ) : base()
	{
		Bounds = new Rectangle( 0, size );

		SetImage( path );
	}

	public void SetImage( string path )
	{
		using var _ = new Stopwatch( "Image.SetImage" );

		var fileBytes = FileSystem.Game.ReadAllBytes( path );
		TextureInfo = Serializer.Deserialize<MochaFile<TextureInfo>>( fileBytes ).Data;

		var spriteData = new Vector4[TextureInfo.Width * TextureInfo.Height];

		for ( int i = 0; i < TextureInfo.MipData[0].Length; i += 4 )
		{
			float x = TextureInfo.MipData[0][i] / 255f;
			float y = TextureInfo.MipData[0][i + 1] / 255f;
			float z = TextureInfo.MipData[0][i + 2] / 255f;
			float w = TextureInfo.MipData[0][i + 3] / 255f;

			spriteData[i / 4] = new Vector4( x, y, z, w );
		}

		if ( TestSprite != null )
			EditorInstance.AtlasBuilder.RemoveSprite( TestSprite );

		TestSprite = EditorInstance.AtlasBuilder.AddSprite( new Point2( (int)TextureInfo.Width, (int)TextureInfo.Height ) );
		TestSprite.SetData( spriteData );

		EditorInstance.AtlasTexture = EditorInstance.AtlasBuilder.Build();
		Graphics.PanelRenderer.UpdateAtlas( EditorInstance.AtlasTexture );
	}

	internal override void Render()
	{
		float aspect = (float)TextureInfo.Width / (float)TextureInfo.Height;
		var bounds = Bounds;
		bounds.Width = bounds.Height * aspect;

		if ( TestSprite != null )
			Graphics.DrawImage( bounds, TestSprite );
	}
}
