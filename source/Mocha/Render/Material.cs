using Mocha.Common.Serialization;

namespace Mocha.Renderer;

public struct Material
{
	public string Path { get; set; }

	public Shader Shader { get; set; }
	public Type UniformBufferType { get; set; }
	public Texture? DiffuseTexture { get; set; }
	public Texture? AlphaTexture { get; set; }
	public Texture? NormalTexture { get; set; }
	public Texture? ORMTexture { get; set; }

	public static Material FromMochaMaterial( string path )
	{
		var fileBytes = File.ReadAllBytes( path );
		var materialFormat = Serializer.Deserialize<MochaFile<MaterialInfo>>( fileBytes );

		return new Material()
		{
			Path = path,
			DiffuseTexture = TextureBuilder.Default.FromMochaTexture( materialFormat.Data.DiffuseTexture ).Build(),
			UniformBufferType = typeof( GenericModelUniformBuffer ),
			Shader = ShaderBuilder.Default.FromMoyaiShader( "content/shaders/pbr.mshdr" ).Build()
		};
	}
}
