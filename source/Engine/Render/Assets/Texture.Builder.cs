using Mocha.Common.Serialization;
using StbImageSharp;

namespace Mocha.Renderer;

public partial class TextureBuilder
{
	private string type = "texture_diffuse";

	private byte[][] data;
	private uint width;
	private uint height;

	private string path;

	private bool isRenderTarget;
	private int mipCount = 1;

	private bool ignoreCache;

	public TextureBuilder()
	{
	}

	public static TextureBuilder Default => new TextureBuilder();
	public static TextureBuilder WorldTexture => new TextureBuilder();
	public static TextureBuilder UITexture => new TextureBuilder();

	private static bool TryGetExistingTexture( string path, out Texture texture )
	{
		var existingTexture = Asset.All.OfType<Texture>().ToList().FirstOrDefault( t => t.Path == path );
		if ( existingTexture != null )
		{
			texture = existingTexture;
			return true;
		}

		texture = default;
		return false;
	}

	public Texture Build()
	{
		if ( TryGetExistingTexture( path, out var existingTexture ) && !ignoreCache )
			return existingTexture;

		return new Texture( path, type, (int)width, (int)height );
	}

	public TextureBuilder WithType( string type = "texture_diffuse" )
	{
		this.type = type;

		return this;
	}

	public TextureBuilder FromMochaTexture( string path )
	{
		if ( TryGetExistingTexture( path, out _ ) )
			return new TextureBuilder() { path = path };

		var fileBytes = FileSystem.Game.ReadAllBytes( path );

		var textureFormat = Serializer.Deserialize<MochaFile<TextureInfo>>( fileBytes );
		this.width = textureFormat.Data.Width;
		this.height = textureFormat.Data.Height;
		this.data = textureFormat.Data.MipData;
		this.mipCount = textureFormat.Data.MipCount;
		this.path = path;

		return this;
	}

	public TextureBuilder FromPath( string path, bool flipY = false )
	{
		if ( TryGetExistingTexture( path, out _ ) )
			return new TextureBuilder() { path = path };

		var fileData = FileSystem.Game.ReadAllBytes( path );
		var image = ImageResult.FromMemory( fileData, ColorComponents.RedGreenBlueAlpha );

		this.data = new[] { image.Data };
		this.width = (uint)image.Width;
		this.height = (uint)image.Height;
		this.path = path;

		return this;
	}

	public TextureBuilder FromData( byte[] data, uint width, uint height )
	{
		this.data = new[] { data };
		this.width = width;
		this.height = height;

		return this;
	}

	public TextureBuilder FromEmpty( uint width, uint height )
	{
		var dataLength = (int)(width * height * 4);

		this.data = new[] { Enumerable.Repeat( (byte)0, dataLength ).ToArray() };
		this.width = width;
		this.height = height;

		return this;
	}

	public TextureBuilder IgnoreCache( bool ignoreCache = true )
	{
		this.ignoreCache = ignoreCache;

		return this;
	}

	public TextureBuilder WithName( string name )
	{
		this.path = name;

		return this;
	}

	public TextureBuilder FromColor( Vector4 vector4 )
	{
		var data = new byte[4];

		data[0] = (byte)(vector4.X * 255).FloorToInt();
		data[1] = (byte)(vector4.Y * 255).FloorToInt();
		data[2] = (byte)(vector4.Z * 255).FloorToInt();
		data[3] = (byte)(vector4.W * 255).FloorToInt();

		return FromData( data, 1, 1 );
	}

	public TextureBuilder FromInternal( string name )
	{
		this.path = name;

		return this;
	}
}
