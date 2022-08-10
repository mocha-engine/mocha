namespace Mocha.Renderer;

[Icon( FontAwesome.Image ), Title( "Texture" )]
public class Texture : Asset
{
	public int Width { get; set; }
	public int Height { get; set; }
	public string Type { get; set; }

	public static TextureBuilder Builder => new();

	internal Texture( string path, string type, int width, int height )
	{
		Path = path;
		Type = type;
		Width = width;
		Height = height;

		All.Add( this );
	}

	public void Delete()
	{
		Asset.All.Remove( this );
	}
}
