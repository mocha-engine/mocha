using Mocha.Common.Serialization;

namespace Mocha.Renderer;

public struct Material
{
	public string Path { get; set; } = "unnamed";

	public Shader Shader { get; set; } = ShaderBuilder.Default.FromMoyaiShader( "content/shaders/pbr.mshdr" ).Build();
	public Type UniformBufferType { get; set; } = typeof( GenericModelUniformBuffer );
	public Texture? DiffuseTexture { get; set; } = TextureBuilder.One;
	public Texture? AlphaTexture { get; set; } = TextureBuilder.One;
	public Texture? NormalTexture { get; set; } = TextureBuilder.Zero;
	public Texture? ORMTexture { get; set; } = TextureBuilder.One;

	public Material()
	{

	}

	public static Material FromMochaMaterial( string path )
	{
		if ( !File.Exists( path ) )
			return new()
			{
				Path = "internal:default"
			};

		var fileBytes = File.ReadAllBytes( path );
		var materialFormat = Serializer.Deserialize<MochaFile<MaterialInfo>>( fileBytes );

		return new()
		{
			Path = path,
			DiffuseTexture = TextureBuilder.Default.FromMochaTexture( materialFormat.Data.DiffuseTexture ).Build()
		};
	}
}
