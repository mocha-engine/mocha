using Mocha.Common.Serialization;
using Mocha.Renderer.UI;
using System.ComponentModel.DataAnnotations;

namespace Mocha.Engine;

internal class Editor
{
	internal static Texture Atlas { get; set; }
	internal static FontData FontData { get; set; }

	private PanelRenderer panelRenderer;
	private List<Panel> panels = new();

	internal static Rectangle FontSubAtlasRect { get; set; }
	internal static Rectangle WhiteAreaSubAtlasRect { get; set; }

	internal static Rectangle SdfAreaNdcCoords { get; set; }

	private Texture CreateAtlas()
	{
		var fontTexture = TextureBuilder.UITexture.FromPath( "core/fonts/baked/qaz.mtex" ).WithNoMips().Build();

		//
		// We want a small quadrant that is just white so that
		// we can use one texture for all UI stuff
		//
		Point2 whiteArea = new Point2( fontTexture.Width, 32 );
		Point2 targetTextureArea = new Point2( fontTexture.Width, fontTexture.Height + whiteArea.Y );

		var targetTextureData = new byte[targetTextureArea.X * targetTextureArea.Y * 4];

		void SetPixel( Point2 pos, Vector4 data )
		{
			int x = pos.X * 4;
			int y = pos.Y * 4;

			targetTextureData[x + (y * targetTextureArea.X)] = (byte)(data.X * 255);
			targetTextureData[x + (y * targetTextureArea.X) + 1] = (byte)(data.Y * 255);
			targetTextureData[x + (y * targetTextureArea.X) + 2] = (byte)(data.Z * 255);
			targetTextureData[x + (y * targetTextureArea.X) + 3] = (byte)(data.W * 255);
		}

		for ( int x = 0; x < whiteArea.X; x++ )
		{
			for ( int y = 0; y < whiteArea.Y; y++ )
			{
				SetPixel( new( x, y ), Vector4.One );
			}
		}

		float RectSDF( Vector2 p, Vector2 b, float r )
		{
			Vector2 absP = new Vector2( MathF.Abs( p.X ), MathF.Abs( p.Y ) );
			Vector2 aSubP = (absP - b);
			if ( aSubP.X < 0.0f )
				aSubP.X = 0.0f;
			if ( aSubP.Y < 0.0f )
				aSubP.Y = 0.0f;

			return aSubP.Length - r;
		}

		for ( int x = 32; x < 64; x++ )
		{
			for ( int y = 0; y < 32; y++ )
			{
				// Rounded box SDF:
				// return length(max(abs(CenterPosition)-Size+Radius,0.0))-Radius;

				Vector2 v = new Vector2( x, y );
				Vector2 origin = new Vector2( 48, 16 );
				Vector2 size = new Vector2( 32, 32 );
				float radius = 0.05f;

				v -= origin;

				float d = RectSDF( v, size / 32.0f, radius );
				d /= 28.0f;
				d = 1.0f - d;

				SetPixel( new( x, y ), new Vector4( d, d, d, 1 ) );
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
		WhiteAreaSubAtlasRect = new Rectangle( 0, 0, whiteArea.X, whiteArea.Y );
		FontSubAtlasRect = new Rectangle( 0, 0, fontTexture.Width, fontTexture.Height );

		SdfAreaNdcCoords = new Rectangle( 32f, 0f, 32f, 32f ) / targetTexture.Size;

		return targetTexture;
	}

	private void AddLabel( string text, float fontSize )
	{
		panels.Add( new Label( text, new Rectangle( cursor, -1 ), fontSize ) );
		cursor.Y += fontSize * 1.5f;
	}

	private void AddSeparator()
	{
		cursor.Y += 8;

		var panel = new Panel( new Rectangle( cursor.X, cursor.Y, Screen.Size.X - 32, 2 ) );
		panel.color = new Vector4( 0.2f, 0.2f, 0.2f, 1 );

		panels.Add( panel );

		cursor.Y += 8;
	}

	private void AddButton( string text )
	{
		cursor.Y += 16f;
		panels.Add( new Button( text, new Rectangle( cursor.X, cursor.Y, 128f, 23f ) ) );
		cursor.Y += 16f;
	}

	Vector2 cursor = new();

	[Event.Hotload]
	public void CreateUI()
	{
		Atlas?.Delete();
		Atlas = CreateAtlas();
		panelRenderer = new( Atlas );

		cursor = new( 16, 16 );

		panels.Clear();

		AddLabel( "Qaz Sans", 80 );
		AddLabel( "Title", 28 );
		AddLabel( "This is a subtitle", 18 );

		AddSeparator();

		AddLabel( "Lorem ipsum dolor sit amet.", 14 );
		AddLabel( "Lorem ipsum dolor sit amet.", 14 );
		AddLabel( "Lorem ipsum dolor sit amet.", 14 );
		AddLabel( "Lorem ipsum dolor sit amet.", 14 );

		AddSeparator();

		AddButton( "Click me" );
		AddButton( "OK" );
		AddButton( "Cancel" );
		AddButton( "click for free iphone" );

		Log.Trace( "CreateUI" );
	}

	internal Editor()
	{
		Event.Register( this );

		CreateUI();
		FontData = FileSystem.Game.Deserialize<FontData>( "core/fonts/baked/qaz.json" );
	}

	internal void Render( Veldrid.CommandList commandList )
	{
		panelRenderer.NewFrame();
		panelRenderer.AddRectangle( new Rectangle( 0f, (Vector2)Screen.Size ), Colors.Gray );

		foreach ( var panel in panels.ToArray() )
		{
			panel.Render( ref panelRenderer );

			if ( panel.rect.Y > Screen.Size.Y )
				panels.Remove( panel );
		}

		panelRenderer.AddRectangle( new Rectangle( Screen.Size.X - Atlas.Width * 2 - 16, 16, Atlas.Width * 2, Atlas.Height * 2 ), Colors.DarkGray );
		panelRenderer.AddRectangle( new Rectangle( Screen.Size.X - Atlas.Width * 2 - 16, 16, Atlas.Width * 2, Atlas.Height * 2 ), new Rectangle( 0, 0, 1, 1 ), Vector4.One );
		// panelRenderer.AddRectangle( new Rectangle( Input.MousePosition, 24f ), new Vector4( 0.5f ) );
		panelRenderer.Draw( commandList );
	}
}
