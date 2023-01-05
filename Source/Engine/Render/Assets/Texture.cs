using Mocha.Common.Serialization;

namespace Mocha.Renderer;

[Icon( FontAwesome.Image ), Title( "Texture" )]
public partial class Texture : Asset
{
	public uint Width { get; set; }
	public uint Height { get; set; }

	internal uint DataWidth { get; set; }
	internal uint DataHeight { get; set; }

	public Glue.Texture NativeTexture { get; set; }
	public Vector2 Size => new Vector2( Width, Height );

	/// <summary>
	/// Loads a texture from an MTEX (compiled) file.
	/// </summary>
	public Texture( string path )
	{
		Path = path;
		All.Add( this );

		var fileBytes = FileSystem.Game.ReadAllBytes( path );

		var textureFormat = Serializer.Deserialize<MochaFile<TextureInfo>>( fileBytes );
		Width = textureFormat.Data.Width;
		Height = textureFormat.Data.Height;

		DataWidth = textureFormat.Data.DataWidth;
		DataHeight = textureFormat.Data.DataHeight;

		var mipData = textureFormat.Data.MipData;
		var mipCount = textureFormat.Data.MipCount;

		NativeTexture = new( DataWidth, DataHeight );

		// Flatten mip data into one big buffer
		List<byte> textureData = new List<byte>();
		for ( var i = 0; i < mipCount; i++ )
		{
			textureData.AddRange( mipData[i] );
		}

		TextureFormat format = TextureFormat.BC3_SRGB;
		if ( path.Contains( "icon" ) )
			format = TextureFormat.BC3_UNORM;
		if ( path.Contains( "normal" ) )
			format = TextureFormat.BC5_UNORM;
		else if ( path.Contains( "font" ) )
			format = TextureFormat.R8G8B8A8_SRGB;
		else if ( path.Contains( "noise" ) )
			format = TextureFormat.R8G8B8A8_SRGB;

		NativeTexture.SetData( DataWidth, DataHeight, (uint)mipCount, textureData.ToInterop(), (int)format );
	}

	/// <summary>
	/// Creates a texture with a specified size, containing RGBA data.
	/// </summary>
	public Texture( uint width, uint height, byte[] data ) : this( width, height )
	{
		NativeTexture.SetData( Width, Height, 1, data.ToInterop(), (int)TextureFormat.R8G8B8A8_SRGB );
	}

	/// <summary>
	/// Creates a blank (no data) texture with a specified size
	/// </summary>
	public Texture( uint width, uint height )
	{
		Path = "Procedural Texture";
		All.Add( this );

		Width = width;
		Height = height;

		NativeTexture = new( width, height );
		NativeTexture.SetData( Width, Height, 1, new byte[Width * Height * 4].ToInterop(), (int)TextureFormat.R8G8B8A8_SRGB );
	}

	public void Copy( uint srcX, uint srcY, uint dstX, uint dstY, uint width, uint height, Texture src )
	{
		NativeTexture.Copy( srcX, srcY, dstX, dstY, width, height, src.NativeTexture );
	}

	public void Delete()
	{
		Asset.All.Remove( this );
	}

	//
	// Texture caching
	// TODO: This should really be handled by the C++ side, but this will do for now
	private static Dictionary<string, Texture> CachedTextures { get; } = new();

	public static Texture FromCache( string fontName )
	{
		if ( CachedTextures.TryGetValue( fontName, out var cachedTexture ) )
		{
			return cachedTexture;
		}

		var loadedTexture = new Texture( fontName );
		return CachedTextures[fontName] = loadedTexture;
	}
}
