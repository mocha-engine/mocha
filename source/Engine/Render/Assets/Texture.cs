using Mocha.Common.Serialization;

namespace Mocha.Renderer;

[Icon( FontAwesome.Image ), Title( "Texture" )]
public partial class Texture : Asset
{
	public uint Width { get; set; }
	public uint Height { get; set; }

	public Glue.Texture NativeTexture { get; set; }
	public Vector2 Size => new Vector2( Width, Height );

	/// <summary>
	/// Loads a texture from an MTEX (compiled) file.
	/// </summary>
	public Texture( string path )
	{
		var fileBytes = FileSystem.Game.ReadAllBytes( path );

		var textureFormat = Serializer.Deserialize<MochaFile<TextureInfo>>( fileBytes );
		Width = textureFormat.Data.Width;
		Height = textureFormat.Data.Height;

		var mipData = textureFormat.Data.MipData;
		var mipCount = textureFormat.Data.MipCount;

		NativeTexture = new();

		// Flatten mip data into one big buffer
		List<byte> textureData = new List<byte>();
		for ( var i = 0; i < mipCount; i++ )
		{
			textureData.AddRange( mipData[i] );
		}

		TextureFormat format = TextureFormat.BC3_SRGB;
		if ( path.Contains( "normal" ) )
			format = TextureFormat.BC5_UNORM;
		else if ( path.Contains( "font" ) )
			format = TextureFormat.R8G8B8A8_SRGB;

		NativeTexture.SetData( Width, Height, (uint)mipCount, textureData.ToInterop(), (int)format );
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
		Width = width;
		Height = height;

		NativeTexture = new();
		NativeTexture.SetData( Width, Height, 1, new byte[Width * Height * 4].ToInterop(), (int)TextureFormat.R8G8B8A8_SRGB );
	}

	internal Texture( string path, int width, int height )
	{
		Path = path;
		Width = (uint)width;
		Height = (uint)height;

		All.Add( this );
	}

	public void Copy( uint srcX, uint srcY, uint dstX, uint dstY, uint width, uint height, Texture src )
	{
		// TODO: This actually just blits because copying does not ignore texture format differences. Is this a performance issue?
		NativeTexture.Blit( srcX, srcY, dstX, dstY, width, height, src.NativeTexture );
	}

	public void Delete()
	{
		Asset.All.Remove( this );
	}
}
