using Mocha.Common.Serialization;

namespace Mocha.Renderer;

[Icon( FontAwesome.FaceGrinStars ), Title( "Material" )]
public class Material : Asset
{
	public Texture? DiffuseTexture { get; set; } = Texture.MissingTexture;
	public Texture? NormalTexture { get; set; } = Texture.Normal;
	public Texture? AmbientOcclusionTexture { get; set; } = Texture.One;
	public Texture? MetalnessTexture { get; set; } = Texture.Zero;
	public Texture? RoughnessTexture { get; set; } = Texture.One;

	public Glue.ManagedMaterial NativeMaterial { get; }

	public Material( string path )
	{
		if ( !FileSystem.Game.Exists( path ) )
		{
			Log.Warning( $"Material '{path}' does not exist" );
			return;
		}

		var fileBytes = FileSystem.Game.ReadAllBytes( path );
		var materialFormat = Serializer.Deserialize<MochaFile<MaterialInfo>>( fileBytes );

		if ( !string.IsNullOrEmpty( materialFormat.Data.DiffuseTexture ) )
			DiffuseTexture = new Texture( materialFormat.Data.DiffuseTexture );

		if ( !string.IsNullOrEmpty( materialFormat.Data.NormalTexture ) )
			NormalTexture = new Texture( materialFormat.Data.NormalTexture );

		if ( !string.IsNullOrEmpty( materialFormat.Data.AmbientOcclusionTexture ) )
			AmbientOcclusionTexture = new Texture( materialFormat.Data.AmbientOcclusionTexture );

		if ( !string.IsNullOrEmpty( materialFormat.Data.MetalnessTexture ) )
			MetalnessTexture = new Texture( materialFormat.Data.MetalnessTexture );

		if ( !string.IsNullOrEmpty( materialFormat.Data.RoughnessTexture ) )
			RoughnessTexture = new Texture( materialFormat.Data.RoughnessTexture );

		unsafe
		{
			fixed ( void* attributes = Vertex.VertexAttributes )
			{
				NativeMaterial = new(
					Vertex.VertexAttributes.Length,
					(IntPtr)attributes,

					DiffuseTexture.NativeTexture.NativePtr,
					NormalTexture.NativeTexture.NativePtr,
					AmbientOcclusionTexture.NativeTexture.NativePtr,
					MetalnessTexture.NativeTexture.NativePtr,
					RoughnessTexture.NativeTexture.NativePtr
				);
			}
		}

		Path = path;
	}
}
