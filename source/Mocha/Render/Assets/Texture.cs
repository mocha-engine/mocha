using Veldrid;

namespace Mocha.Renderer;

[Icon( FontAwesome.Image ), Title( "Texture" )]
public class Texture : Asset
{
	public int Width { get; set; }
	public int Height { get; set; }
	public string Type { get; set; }

	public Veldrid.Texture VeldridTexture { get; }
	public Veldrid.TextureView VeldridTextureView { get; }

	public static TextureBuilder Builder => new();

	public Vector2 Size => new Vector2( Width, Height );

	internal Texture( string path, Veldrid.Texture texture, Veldrid.TextureView textureView, string type, int width, int height )
	{
		Path = path;
		VeldridTexture = texture;
		VeldridTextureView = textureView;
		Type = type;
		Width = width;
		Height = height;

		All.Add( this );
	}

	public void Delete()
	{
		Asset.All.Remove( this );

		VeldridTexture.Dispose();
		VeldridTextureView.Dispose();
	}

	public void Update( byte[] data, int x, int y, int width, int height )
	{
		Device.UpdateTexture( VeldridTexture, data, (uint)x, (uint)y, 0, (uint)width, (uint)height, 1, 0, 0 );
	}
}
