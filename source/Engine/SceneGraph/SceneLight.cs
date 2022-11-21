using Mocha.Common.World;

namespace Mocha.Renderer;

public class SceneLight : SceneObject
{
	public float Intensity { get; set; } = 1.0f;
	public Vector4 Color { get; set; } = new Vector4( 1, 1, 1, 1 );

	internal Texture DepthTexture { get; set; }

	internal Matrix4x4 ViewMatrix { get; set; }
	internal Matrix4x4 ProjMatrix { get; set; }

	public SceneLight( IEntity entity )
	{
	}
}
