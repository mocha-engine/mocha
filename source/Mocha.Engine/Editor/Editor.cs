using Mocha.Common.Serialization;
using Mocha.Renderer.UI;

namespace Mocha.Engine;

internal class Editor
{
	internal static Texture Atlas { get; set; }
	internal static FontData FontData { get; set; }

	private PanelRenderer panelRenderer;
	private List<Panel> panels = new();

	private Rectangle fontSubAtlasRect;
	private Rectangle whiteAreaSubAtlasRect;

	private Texture CreateAtlas()
	{
		var fontTexture = TextureBuilder.UITexture.FromPath( "core/fonts/baked/qaz.mtex" ).WithNoMips().Build();

		//
		// We want a small quadrant that is just white so that
		// we can use one texture for all UI stuff
		//
		Point2 whiteArea = new Point2( fontTexture.Width, 1 );
		Point2 targetTextureArea = new Point2( fontTexture.Width, fontTexture.Height + whiteArea.Y );

		var targetTextureData = new byte[targetTextureArea.X * targetTextureArea.Y * 4];

		for ( int x = 0; x < whiteArea.X * 4; x += 4 )
		{
			for ( int y = 0; y < whiteArea.Y * 4; y += 4 )
			{
				targetTextureData[x + (y * targetTextureArea.X)] = 255;
				targetTextureData[x + (y * targetTextureArea.X) + 1] = 255;
				targetTextureData[x + (y * targetTextureArea.X) + 2] = 255;
				targetTextureData[x + (y * targetTextureArea.X) + 3] = 255;
			}
		}

		var targetTexture = TextureBuilder.UITexture.FromEmpty( (uint)targetTextureArea.X, (uint)targetTextureArea.Y ).Build();

		targetTexture.Update( targetTextureData, 0, 0, whiteArea.X, whiteArea.Y );

		// Really roundabout way of loading raw texture data just so that we can put it in a texture again
		// TODO: Make all this much better.. it sucks.
		var fileBytes = FileSystem.Game.ReadAllBytes( "core/fonts/baked/qaz.mtex" );
		var fontTextureBytes = Serializer.Deserialize<MochaFile<TextureInfo>>( fileBytes ).Data.MipData[0];
		targetTexture.Update( fontTextureBytes, 0, whiteArea.Y, fontTexture.Width, fontTexture.Height );

		//
		// Save off sub-atlas rectangles so that we can use them in other calculations later
		//
		whiteAreaSubAtlasRect = new Rectangle( 0, 0, whiteArea.X, whiteArea.Y );
		fontSubAtlasRect = new Rectangle( 0, whiteArea.Y, fontTexture.Width, fontTexture.Height );

		return targetTexture;
	}

	private void AddLabel( string text, float fontSize )
	{
		layoutY += fontSize * 1.5f;
		panels.Add( new Label( text, new Rectangle( 16, layoutY, -1, -1 ), fontSize ) );
	}

	float layoutY;

	[Event.Hotload]
	public void CreateUI()
	{
		layoutY = 0;
		panels.Clear();

		AddLabel( "Title", 64 );
		AddLabel( "This is a subtitle", 32 );

		layoutY += 14;

		AddLabel( "Lorem ipsum dolor sit amet.", 14 );
		AddLabel( "Lorem ipsum dolor sit amet.", 14 );
		AddLabel( "Lorem ipsum dolor sit amet.", 14 );
		AddLabel( "Lorem ipsum dolor sit amet.", 14 );

		Log.Trace( "CreateUI" );
	}

	internal Editor()
	{
		Event.Register( this );
		
		Atlas = CreateAtlas();
		panelRenderer = new( Atlas );
		FontData = FileSystem.Game.Deserialize<FontData>( "core/fonts/baked/qaz.json" );

		CreateUI();
	}

	internal void Render( Veldrid.CommandList commandList )
	{
		panelRenderer.NewFrame();
		panelRenderer.AddRectangle( new Rectangle( 0f, (Vector2)Screen.Size ), new Vector3( 0.15f ) );

		foreach ( var panel in panels.ToArray() )
		{
			panel.Render( ref panelRenderer );

			if ( panel.rect.Y > Screen.Size.Y )
				panels.Remove( panel );
		}

		panelRenderer.AddRectangle( new Rectangle( Input.MousePosition, 24f ), new Vector3( 0.5f ) );
		panelRenderer.Draw( commandList );
	}
}
