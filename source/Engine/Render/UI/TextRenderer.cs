namespace Mocha.Renderer.UI;

public class TextureStitcher
{
	public Texture Texture { get; private set; }

	public TextureStitcher( int width, int height )
	{
		Texture = new Texture( (uint)width, (uint)height );
	}

	public void AddTexture( Vector2 pos, Vector2 offset, Vector2 size, Texture texture )
	{
	}

	public void AddTexture( Vector2 pos, Texture texture )
	{
		AddTexture( pos, Vector2.Zero, new Vector2( texture.Width, texture.Height ), texture );
	}
}
