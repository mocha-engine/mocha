namespace Mocha.Renderer;

public class SceneLight : SceneObject
{
	public float Intensity { get; set; } = 1.0f;
	public RgbaFloat Color { get; set; } = new RgbaFloat( 1, 1, 1, 1 );
	public SceneLight( IEntity entity ) : base( entity )
	{
	}
}
