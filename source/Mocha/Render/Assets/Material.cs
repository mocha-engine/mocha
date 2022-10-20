using Mocha.Common.Serialization;

namespace Mocha.Renderer;

[Icon( FontAwesome.FaceGrinStars ), Title( "Material" )]
public class Material : Asset
{
	public Shader Shader { get; set; } = new ShaderBuilder().FromPath( "core/shaders/pbr.mshdr" ).Build();
	public Type UniformBufferType { get; set; } = typeof( GenericModelUniformBuffer );
	public Texture? DiffuseTexture { get; set; } = TextureBuilder.MissingTexture;
	public Texture? AlphaTexture { get; set; } = TextureBuilder.One;
	public Texture? NormalTexture { get; set; } = TextureBuilder.Zero;
	public Texture? ORMTexture { get; set; } = TextureBuilder.One;

	public Material()
	{
		All.Add( this );
	}

	public Material( string path ) : this()
	{
		if ( !FileSystem.Game.Exists( path ) )
		{
			Path = "internal:default";
			return;
		}

		var fileBytes = FileSystem.Game.ReadAllBytes( path );
		var materialFormat = Serializer.Deserialize<MochaFile<MaterialInfo>>( fileBytes );

		Path = path;
		DiffuseTexture = new Texture( materialFormat.Data.DiffuseTexture );
	}

	[Obsolete( "Use ctor" )]
	public static Material FromPath( string path )
	{
		return new Material( path );
	}
}
