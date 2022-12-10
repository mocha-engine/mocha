using Mocha.Common.Serialization;

namespace Mocha.Renderer;

[Icon( FontAwesome.FaceGrinStars ), Title( "Material" )]
public class Material : Asset
{
	public Type UniformBufferType { get; set; } = typeof( GenericModelUniformBuffer );
	public Texture? DiffuseTexture { get; set; } = Texture.One;
	public Texture? AlphaTexture { get; set; } = Texture.One;
	public Texture? NormalTexture { get; set; } = Texture.Zero;
	public Texture? ORMTexture { get; set; } = Texture.One;

	public Material( string path )
	{
		if ( !FileSystem.Game.Exists( path ) )
		{
			Log.Warning( $"Material '{path}' does not exist" );
			return;
		}

		var fileBytes = FileSystem.Game.ReadAllBytes( path );
		var materialFormat = Serializer.Deserialize<MochaFile<MaterialInfo>>( fileBytes );

		var diffuseTexture = new Texture( materialFormat.Data.DiffuseTexture );

		Path = path;
		DiffuseTexture = diffuseTexture;
	}
}
