namespace Mocha.Engine.Editor;

internal class Image : Widget
{
	public Vector4 Color { get; set; } = new Vector4( 1, 1, 1, 1 );
	private Texture Texture { get; set; }

	internal Image( Vector2 size, string path ) : base()
	{
		Bounds = new Rectangle( 0, size );

		SetImage( path );
	}

	public void SetImage( string path )
	{
		Texture = new Texture( path );
	}

	internal override void Render()
	{
		float aspect = (float)Texture.Width / (float)Texture.Height;
		var bounds = Bounds;
		bounds.Width = bounds.Height * aspect;

		Graphics.DrawTexture( bounds, Texture ?? TextureBuilder.MissingTexture );
	}
}
