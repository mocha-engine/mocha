using Mocha.Common.Serialization;
using Mocha.Renderer.UI;
using Veldrid.OpenGLBinding;
using static Mocha.Engine.Editor.Font;

namespace Mocha.Engine.Editor;

internal class EditorInstance
{
	internal static Texture AtlasTexture { get; set; }
	internal static Font.Data FontData { get; set; }

	internal static Sprite FontSprite { get; set; }
	internal static Sprite WhiteSprite { get; set; }
	internal static Sprite SDFSprite { get; set; }

	private VerticalLayout Layout { get; set; }

	private PanelRenderer panelRenderer;

	private const string Font = "qaz";
	private const string Lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam sed pharetra lorem. Aliquam eget tristique turpis, eget tristique mi. Nullam et ex vitae mauris dapibus luctus nec vel nisl. Nam venenatis vel orci a sagittis.";

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

	[Event.Hotload]
	public void CreateUI()
	{
		//
		// Clean up existing widgets & panels
		//
		Clear();

		//
		// Everything has to go inside a layout otherwise it won't be rendered
		//
		Layout = new VerticalLayout();
		Layout.Spacing = 4;
		Layout.Margin = 16;

		//
		// Text rendering
		//
		Layout.AddWidget( new Label( "The quick brown fox", 64 ) );
		Layout.AddWidget( new Label( "This is a test", 32 ) );
		Layout.AddWidget( new Label( Lipsum, 12 ) );

		Layout.AddSpacing( 8f );

		//
		// Theme switcher (dropdown)
		//
		var dropdown = new Dropdown( "Dark Theme" );
		dropdown.AddOption( "Dark Theme" );
		dropdown.AddOption( "Light Theme" );
		dropdown.OnSelected += SwitchTheme;
		Layout.AddWidget( dropdown );

		//
		// Different button lengths (sizing test)
		//
		Layout.AddWidget( new Button( "Another awesome button" ) );
		Layout.AddWidget( new Button( "I like big butts" ) );
		Layout.AddWidget( new Button( "OK" ) );
		Layout.AddWidget( new Button( "A" ) );
		Layout.AddWidget( new Button( "QWERTY" ) );
		Layout.AddWidget( new Button( "I am a really long button with some really long text inside it" ) );
	}

	internal EditorInstance()
	{
		Event.Register( this );
		FontData = FileSystem.Game.Deserialize<Font.Data>( $"core/fonts/baked/{Font}.json" );

		CreateUI();
	}

	internal void Render( Veldrid.CommandList commandList )
	{
		panelRenderer.NewFrame();

		panelRenderer.AddRectangle( new Rectangle( 0, (Vector2)Screen.Size ), ITheme.Current.BackgroundColor );

		foreach ( var layout in VerticalLayout.All.ToArray() )
		{
			layout.Render( panelRenderer );
		}

		panelRenderer.Draw( commandList );
	}

	internal void SwitchTheme( int newSelection )
	{
		Log.Trace( newSelection );

		if ( newSelection == 0 )
			ITheme.Current = new DarkTheme();
		else
			ITheme.Current = new LightTheme();

		Window.Current.SetDarkMode( ITheme.Current is DarkTheme );
	}

	internal void Clear()
	{
		// Rebuild atlas (TODO: This should be automatic / transparent)
		BuildAtlas();
		panelRenderer = new( AtlasTexture );

		Layout?.Delete();
		Layout = null;
	}
}
