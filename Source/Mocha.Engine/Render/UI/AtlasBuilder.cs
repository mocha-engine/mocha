namespace Mocha;

public class AtlasBuilder
{
	private List<(Point2 Position, Texture Texture)> TextureCache { get; } = new();

	public Texture Texture { get; private set; }
	public Action OnBuild { get; set; }

	private uint _rowHeight = 0;
	private Vector2 _cursor = new();

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

		if ( _rowHeight < texture.Height )
			_rowHeight = texture.Height;

		if ( _cursor.X + texture.Width > Size )
		{
			_cursor.X = 0;
			_cursor.Y += _rowHeight;
			_rowHeight = 0;
		}

		//
		// Copy the texture into the atlas
		//
		Point2 pos = new Point2( (int)_cursor.X, (int)_cursor.Y );

		Texture.Copy( 0, 0, (uint)pos.X, (uint)pos.Y, texture.Width, texture.Height, texture );

		TextureCache.Add( (pos, texture) );
		_cursor.X += texture.Width;

		OnBuild?.Invoke();
		return pos;
	}
}
