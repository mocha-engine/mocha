namespace Mocha.Renderer.UI;

public class AtlasBuilder
{
	private List<(Point2 Position, Texture Texture)> TextureCache { get; } = new();

	public Texture Texture { get; private set; }
	public Action OnBuild;

	public AtlasBuilder()
	{
	}

	private Point2 CalculateSize()
	{
		//
		// Get total texture size
		//
		int width = 0;
		int height = 0;

		foreach ( var item in TextureCache )
		{
			var texture = item.Texture;

			width += texture.Width;

			if ( texture.Height > height )
				height = texture.Height;
		}

		return new Point2( width, height );
	}

	public Point2 AddOrGetTexture( Texture texture )
	{
		if ( TextureCache.Any( x => x.Texture == texture ) )
		{
			return TextureCache.First( x => x.Texture == texture ).Position;
		}

		Log.Trace( $"Adding new texture '{texture.Path}' to atlas" );

		Point2 pos;
		int x = 0;
		int y = 0;

		TextureCache.ForEach( t => x += t.Texture.Width );
		pos = new Point2( x, y );

		TextureCache.Add( (pos, texture) );

		Build();
		return pos;
	}

	private void Build()
	{
		var _ = new Stopwatch( "AtlasBuilder.Build" );

		var (width, height) = CalculateSize();
		Log.Trace( $"Building atlas with size {(width, height)}" );

		var result = Texture.Builder.FromEmpty( (uint)width, (uint)height ).IgnoreCache().Build();

		var commandList = Device.ResourceFactory.CreateCommandList();
		commandList.Name = "AtlasTexture update";
		commandList.Begin();

		foreach ( var item in TextureCache )
		{
			var texture = item.Texture;
			var pos = item.Position;

			//
			// Copy the texture into the atlas
			//
			Log.Trace( $"Copying texture {texture.Path} into texture atlas.." );

			commandList.CopyTexture( texture.VeldridTexture,
				 0,
				 0,
				 0,
				 0,
				 0,
				 result.VeldridTexture,
				 (uint)pos.X,
				 (uint)pos.Y,
				 0,
				 0,
				 0,
				 (uint)texture.Width,
				 (uint)texture.Height,
				 1,
				 1 );
		}

		commandList.End();
		Device.SubmitCommands( commandList );

		Texture = result;
		OnBuild?.Invoke();
	}
}
