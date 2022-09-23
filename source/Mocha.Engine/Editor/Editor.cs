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

	private VerticalLayout RootLayout { get; set; }

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
		// Everything has to go inside a layout otherwise they'll go in funky places
		//
		RootLayout = new VerticalLayout();
		RootLayout.Spacing = 2;
		RootLayout.Margin = 16;
		RootLayout.Size = (Vector2)Screen.Size - 32f;

		//
		// Text rendering
		//
		RootLayout.Add( new Label( "The quick brown fox", 64 ) );
		RootLayout.Add( new Label( "This is a test", 32 ) );
		RootLayout.Add( new Label( Lipsum, 12 ) );

		RootLayout.AddSpacing( 4f );

		//
		// Theme switcher (dropdown)
		//
		var themeSwitcher = new Dropdown( "Dark Theme" );
		themeSwitcher.AddOption( "Dark Theme" );
		themeSwitcher.AddOption( "Light Theme" );
		themeSwitcher.AddOption( "Test Theme" );
		themeSwitcher.OnSelected += SwitchTheme;
		RootLayout.Add( themeSwitcher );

		//
		// Different button lengths (sizing test)
		//
		RootLayout.Add( new Button( "Another awesome button" ) );
		RootLayout.Add( new Button( "I like big butts", () =>
		{
			RootLayout.Add( new Label( "Hello!!!!!!", 32 ) );
		} ) );
		RootLayout.Add( new Button( "OK" ) );
		RootLayout.Add( new Button( "A" ) );
		RootLayout.Add( new Button( "QWERTY" ) );
		RootLayout.Add( new Button( "I am a really long button with some really long text inside it" ) );
		RootLayout.Add( new Button( "Stretch" ), true );

		//
		// Test dropdown
		//
		var dropdown = new Dropdown( "Hello" );
		dropdown.AddOption( "Hello" );
		dropdown.AddOption( "World" );
		dropdown.AddOption( "This is a test" );
		dropdown.AddOption( "I am a really long dropdown entry" );
		dropdown.AddOption( "Poo" );
		RootLayout.Add( dropdown );
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

		var widgets = Widget.All.Where( x => x.Visible ).OrderBy( x => x.ZIndex ).ToList();
		var mouseOverWidgets = widgets.Where( x => x.Bounds.Contains( Input.MousePosition ) );

		foreach ( var widget in widgets )
		{
			widget.InputFlags = PanelInputFlags.None;
		}

		if ( mouseOverWidgets.Any() )
		{
			var focusedWidget = mouseOverWidgets.Last();
			focusedWidget.InputFlags |= PanelInputFlags.MouseOver;

			if ( Input.MouseLeft )
			{
				focusedWidget.InputFlags |= PanelInputFlags.MouseDown;
			}
		}

		foreach ( var widget in widgets )
		{
			widget.Render( ref panelRenderer );
		}

		panelRenderer.Draw( commandList );
	}

	internal void SwitchTheme( int newSelection )
	{
		Log.Trace( newSelection );

		if ( newSelection == 0 )
			ITheme.Current = new DarkTheme();
		else if ( newSelection == 1 )
			ITheme.Current = new LightTheme();
		else
			ITheme.Current = new TestTheme();

		Window.Current.SetDarkMode( ITheme.Current is not LightTheme );
	}

	internal void Clear()
	{
		// Rebuild atlas (TODO: This should be automatic / transparent)
		BuildAtlas();
		panelRenderer = new( AtlasTexture );

		RootLayout?.Delete();
		RootLayout = null;

		Widget.All.ToList().ForEach( x => x.Delete() );
		Widget.All.Clear();
	}

	[Event.Hotload]
	public void OnHotload()
	{
		CreateUI();
	}

	[Event.Window.Resized]
	public void OnResize( Point2 _ )
	{
		CreateUI();
	}
}
