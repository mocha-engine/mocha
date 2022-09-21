using Mocha.Common.Serialization;
using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class EditorInstance
{
	internal static Texture AtlasTexture { get; set; }
	internal static Font.Data FontData { get; set; }

	internal static Sprite FontSprite { get; set; }
	internal static Sprite WhiteSprite { get; set; }
	internal static Sprite SDFSprite { get; set; }

	private PanelRenderer panelRenderer;
	private List<Panel> panels = new();

	private string font = "qaz";

	private void BuildAtlas()
	{
		AtlasTexture?.Delete();
		AtlasBuilder atlasBuilder = new();

		var fileBytes = FileSystem.Game.ReadAllBytes( $"core/fonts/baked/{font}.mtex" );
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

	private void AddRoundedPanel( Vector2 size )
	{
		cursor.Y += 8;

		var panel = new RoundedPanel( new Rectangle( cursor.X, cursor.Y, size.X, size.Y ), 8f );
		panels.Add( panel );

		cursor.Y += size.Y + 8;
	}

	private void AddLabel( string text, float fontSize )
	{
		panels.Add( new Label( text, new Rectangle( cursor, -1 ), fontSize ) );
		cursor.Y += fontSize * 1.5f;
	}

	private void AddSeparator()
	{
		cursor.Y += 16f;

		var panel = new Panel( new Rectangle( cursor.X, cursor.Y, Screen.Size.X - 32, 2 ) );
		panel.Color = ITheme.Current.Border;

		panels.Add( panel );

		cursor.Y += 16f;
	}

	private void AddButton( string text, Action? onClick = null )
	{
		cursor.Y += 8f;

		var b = new Button( text, new Rectangle( cursor.X, cursor.Y, 128f, 23f ) );
		b.onClick += onClick;

		panels.Add( b );

		cursor.Y += 24f;
	}

	Vector2 cursor = new();

	[Event.Hotload]
	public void CreateUI()
	{
		BuildAtlas();

		panelRenderer = new( AtlasTexture );

		cursor = new( 16, 16 );

		panels.Clear();

		AddLabel( "Qaz Sans", 80 );
		AddLabel( "Title", 28 );
		AddLabel( "This is a subtitle", 18 );

		AddButton( "Switch Theme", () =>
		{
			if ( ITheme.Current is DarkTheme )
				ITheme.Current = new LightTheme();
			else
				ITheme.Current = new DarkTheme();

			Window.Current.SetDarkMode( ITheme.Current is DarkTheme );

			CreateUI();
		} );

		AddSeparator();

		AddLabel( "(12) The quick brown fox jumps over the lazy dog.", 12 * 1.333f );
		AddLabel( "(18) The quick brown fox jumps over the lazy dog.", 18 * 1.333f );
		AddLabel( "(24) The quick brown fox jumps over the lazy dog.", 24 * 1.333f );
		AddLabel( "(36) The quick brown fox jumps over the lazy dog.", 36 * 1.333f );

		AddSeparator();

		AddButton( "Click me" );
		AddButton( "OK" );
		AddButton( "Cancel" );
		AddButton( "click for free iphone" );

		AddSeparator();

		AddRoundedPanel( new( 128, 64 ) );
		AddRoundedPanel( new( 256, 32 ) );
		AddRoundedPanel( new( 64, 64 ) );

		Log.Trace( "CreateUI" );
	}

	internal EditorInstance()
	{
		Event.Register( this );

		CreateUI();
		FontData = FileSystem.Game.Deserialize<Font.Data>( $"core/fonts/baked/{font}.json" );
	}

	internal void Render( Veldrid.CommandList commandList )
	{
		panelRenderer.NewFrame();
		panelRenderer.AddRectangle( new Rectangle( 0f, (Vector2)Screen.Size ), ITheme.Current.BackgroundColor );

		panels.ForEach( x => x.Render( ref panelRenderer ) );

		panelRenderer.Draw( commandList );
	}
}
