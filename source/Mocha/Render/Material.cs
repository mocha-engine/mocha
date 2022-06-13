using Veldrid;

namespace Mocha;

public struct Material
{
	public Shader Shader { get; set; }
	public Type UniformBufferType { get; set; }
	public Texture? DiffuseTexture { get; set; }
	public Texture? SpecularTexture { get; set; }
	public Texture? NormalTexture { get; set; }
	public Texture? EmissiveTexture { get; set; }
	public Texture? ORMTexture { get; set; }

	public bool IsDirty => (DiffuseTexture?.IsDirty ?? false)
		|| (SpecularTexture?.IsDirty ?? false)
		|| (NormalTexture?.IsDirty ?? false)
		|| (EmissiveTexture?.IsDirty ?? false)
		|| (ORMTexture?.IsDirty ?? false);

	public void GenerateMipmaps( CommandList commandList )
	{
		if ( DiffuseTexture?.IsDirty ?? false )
			DiffuseTexture.GenerateMipmaps( commandList );

		if ( SpecularTexture?.IsDirty ?? false )
			SpecularTexture.GenerateMipmaps( commandList );

		if ( NormalTexture?.IsDirty ?? false )
			NormalTexture.GenerateMipmaps( commandList );

		if ( EmissiveTexture?.IsDirty ?? false )
			EmissiveTexture.GenerateMipmaps( commandList );

		if ( ORMTexture?.IsDirty ?? false )
			ORMTexture.GenerateMipmaps( commandList );
	}
}
