namespace Mocha;

public class AtlasBuilder
{
	private List<(Point2 Position, Texture Texture)> TextureCache { get; } = new();

	public Texture Texture { get; private set; }
	public Action OnBuild { get; set; }

	private uint RowHeight = 0;
	private Vector2 Cursor = new();
	private const uint Size = 4096;

	public AtlasBuilder()
	{
		Texture = new Texture( Size, Size );
	}

	public Point2 AddOrGetTexture( Texture texture )
	{
		if ( TextureCache.Any( x => x.Texture == texture ) )
		{
			return TextureCache.First( x => x.Texture == texture ).Position;
		}

		if ( RowHeight < texture.Height )
			RowHeight = texture.Height;

		if ( Cursor.X + texture.Width > Size )
		{
			Cursor.X = 0;
			Cursor.Y += RowHeight;
			RowHeight = 0;
		}

		//
		// Copy the texture into the atlas
		//
		Point2 pos = new Point2( (int)Cursor.X, (int)Cursor.Y );

		Texture.Copy( 0, 0, (uint)pos.X, (uint)pos.Y, texture.Width, texture.Height, texture );

		TextureCache.Add( (pos, texture) );
		Cursor.X += texture.Width;

		OnBuild?.Invoke();
		return pos;
	}
}
