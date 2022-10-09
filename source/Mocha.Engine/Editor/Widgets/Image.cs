namespace Mocha.Engine.Editor;

internal class Image : Widget
{
	public Vector4 Color { get; set; } = new Vector4( 1, 1, 1, 1 );
	public Texture Texture { get; set; }

	internal Image( Vector2 size ) : base()
	{
		Bounds = new Rectangle( 0, size );
	}

	internal override void Render()
	{
		Graphics.DrawImage( Bounds );
	}
}
