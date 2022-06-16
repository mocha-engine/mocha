using Veldrid;

namespace Mocha;

public struct Material
{
	public Shader Shader { get; set; }
	public Type UniformBufferType { get; set; }
	public Texture? DiffuseTexture { get; set; }
	public Texture? AlphaTexture { get; set; }
	public Texture? NormalTexture { get; set; }
	public Texture? ORMTexture { get; set; }
}
