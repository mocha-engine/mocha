using Mocha.Common.Serialization;
using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;
partial class EditorInstance
{
	internal static Texture AtlasTexture { get; set; }
	internal static Font.Data FontData { get; set; }

	internal static Sprite FontSprite { get; set; }
	internal static Sprite WhiteSprite { get; set; }
	internal static Sprite SDFSprite { get; set; }
	
	private void BuildAtlas()
	{
		AtlasTexture?.Delete();
		AtlasBuilder atlasBuilder = new();

		var fileBytes = FileSystem.Game.ReadAllBytes( $"core/fonts/baked/{Font}.mtex" );
		var fontTextureInfo = Serializer.Deserialize<MochaFile<TextureInfo>>( fileBytes ).Data;

		//
		// Resize the atlas to fit everything we need
		//
		WhiteSprite = atlasBuilder.AddSprite( new Point2( 32, 32 ) );
		SDFSprite = atlasBuilder.AddSprite( new Point2( 32, 32 ) );
		FontSprite = atlasBuilder.AddSprite( new Point2( (int)fontTextureInfo.Width, (int)fontTextureInfo.Height ) );

		//
		// Set sprite data
		//

		// White box
		{
			var whiteSpriteData = new Vector4[32 * 32];
			Array.Fill( whiteSpriteData, Vector4.One );
			WhiteSprite.SetData( whiteSpriteData );
		}

		// Rounded rectangle SDF
		{
			// TODO
			var sdfSpriteData = new Vector4[32 * 32];

			float RectSDF( Vector2 p, Vector2 b, float r )
			{
				return (p.Length) - r;

				Vector2 absP = new Vector2( MathF.Abs( p.X ), MathF.Abs( p.Y ) );
				Vector2 aSubP = (absP - b);
				if ( aSubP.X < 0.0f )
					aSubP.X = 0.0f;
				if ( aSubP.Y < 0.0f )
					aSubP.Y = 0.0f;

				return aSubP.Length - r;
			}

			for ( int x = 0; x < 32; x++ )
			{
				for ( int y = 0; y < 32; y++ )
				{
					// Rounded box SDF:
					// return length(max(abs(CenterPosition)-Size+Radius,0.0))-Radius;

					Vector2 v = new Vector2( x, y );
					Vector2 center = new Vector2( 16, 16 );
					Vector2 size = new Vector2( 32, 32 );
					float radius = 16f;

					v -= center;

					float d = RectSDF( v, size / 32f, radius );
					d /= 32f;

					sdfSpriteData[x + (y * 32)] = new Vector4( d, d, d, 1 );
				}
			}

			SDFSprite.SetData( sdfSpriteData );
		}

		// Font data
		{
			var fontSpriteData = new Vector4[fontTextureInfo.Width * fontTextureInfo.Height];

			for ( int i = 0; i < fontTextureInfo.MipData[0].Length; i += 4 )
			{
				float x = fontTextureInfo.MipData[0][i] / 255f;
				float y = fontTextureInfo.MipData[0][i + 1] / 255f;
				float z = fontTextureInfo.MipData[0][i + 2] / 255f;
				float w = fontTextureInfo.MipData[0][i + 3] / 255f;

				fontSpriteData[i / 4] = new Vector4( x, y, z, w );
			}

			FontSprite.SetData( fontSpriteData );
		}

		//
		// Build final texture
		//
		AtlasTexture = atlasBuilder.Build();
	}
}
