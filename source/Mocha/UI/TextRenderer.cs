namespace Mocha.Renderer.UI;

public class TextureStitcher
{
	public Texture Texture { get; private set; }

	public TextureStitcher( int width, int height )
	{
		Texture = Texture.Builder.FromEmpty( (uint)width, (uint)height ).Build();
	}

	public void AddTexture( Vector2 pos, Vector2 offset, Vector2 size, Texture texture )
	{
		var commandList = Device.ResourceFactory.CreateCommandList();
		commandList.Name = "TextureStitcher update";
		commandList.Begin();

		commandList.CopyTexture(
			texture.VeldridTexture,
			(uint)offset.X,
			(uint)offset.Y,
			0,
			0,
			0,
			Texture.VeldridTexture,
			(uint)pos.X,
			(uint)pos.Y,
			0,
			0,
			0,
			(uint)size.X,
			(uint)size.Y,
			1,
			1 );

		commandList.End();
		Device.SubmitCommands( commandList );
	}

	public void AddTexture( Vector2 pos, Texture texture )
	{
		AddTexture( pos, Vector2.Zero, new Vector2( texture.Width, texture.Height ), texture );
	}
}
