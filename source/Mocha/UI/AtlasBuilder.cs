namespace Mocha.Renderer.UI;

public class AtlasBuilder
{
	private List<(Point2 Position, Texture Texture)> TextureCache { get; } = new();

	public Texture Texture { get; private set; }

	public Action OnBuild;

	private int RowHeight = 0;
	private Vector2 Cursor = new();

	private const uint Size = 8192;

	public AtlasBuilder()
	{
		Texture = new TextureBuilder().FromEmpty( Size, Size ).IgnoreCache().Build();
	}

	public Point2 AddOrGetTexture( Texture texture )
	{
		if ( TextureCache.Any( x => x.Texture == texture ) )
		{
			return TextureCache.First( x => x.Texture == texture ).Position;
		}

		if ( RowHeight < texture.Height )
			RowHeight = texture.Height;

		if ( Cursor.X + texture.Size.X > Size )
		{
			Cursor.X = 0;
			Cursor.Y += RowHeight;
			RowHeight = 0;
		}

		var commandList = Device.ResourceFactory.CreateCommandList();
		commandList.Name = "AtlasTexture update";
		commandList.Begin();

		//
		// Copy the texture into the atlas
		//
		Point2 pos = new Point2( (int)Cursor.X, (int)Cursor.Y );

		commandList.CopyTexture( texture.VeldridTexture,
			 0,
			 0,
			 0,
			 0,
			 0,
			 Texture.VeldridTexture,
			 (uint)pos.X,
			 (uint)pos.Y,
			 0,
			 0,
			 0,
			 (uint)texture.Width,
			 (uint)texture.Height,
			 1,
			 1 );

		TextureCache.Add( (pos, texture) );

		Cursor.X += texture.Size.X;

		commandList.End();
		Device.SubmitCommands( commandList );

		OnBuild?.Invoke();
		return pos;
	}
}
