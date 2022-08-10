using Mocha.Common.Serialization;

namespace Mocha.Renderer;

[Icon( FontAwesome.FaceGrinStars ), Title( "Material" )]
public class Material : Asset
{
	public Shader Shader { get; set; } = ShaderBuilder.Default.FromMoyaiShader( "shaders/simple.mshdr" ).Build();
	public Type UniformBufferType { get; set; } = typeof( GenericModelUniformBuffer );
	public Texture? DiffuseTexture { get; set; } = TextureBuilder.One;
	public Texture? AlphaTexture { get; set; } = TextureBuilder.One;
	public Texture? NormalTexture { get; set; } = TextureBuilder.Zero;
	public Texture? ORMTexture { get; set; } = TextureBuilder.One;

	public Material()
	{
		All.Add( this );
	}

	public static Material FromMochaMaterial( string path )
	{
		if ( !FileSystem.Game.Exists( path ) )
		{
			return new()
			{
				Path = "internal:default"
			};
		}

		var fileBytes = FileSystem.Game.ReadAllBytes( path );
		var materialFormat = Serializer.Deserialize<MochaFile<MaterialInfo>>( fileBytes );

		return new()
		{
			Path = path,
			DiffuseTexture = TextureBuilder.Default.FromMochaTexture( materialFormat.Data.DiffuseTexture ).Build()
		};
	}
}
