namespace Mocha;

[Icon( FontAwesome.FaceGrinStars ), Title( "Material" )]
public class Material : Asset
{
	public Dictionary<string, Texture> Textures { get; set; } = new();

	public Glue.Material NativeMaterial { get; private set; }

	private Material() { }

	/// <summary>
	/// Loads a material from an MMAT (compiled) file.
	/// </summary>
	public Material( string path )
	{
		Path = path;

		// todo: We should really *not* be doing this but I can't be bothered
		// to go through and upgrade every single material right now
		MochaFile<Dictionary<string, string>> textureBindings = new();

		if ( FileSystem.Mounted.Exists( path ) )
		{
			var fileBytes = FileSystem.Mounted.ReadAllBytes( path );
			textureBindings = Serializer.Deserialize<MochaFile<Dictionary<string, string>>>( fileBytes );
		}
		else
		{
			Log.Warning( $"Material '{path}' does not exist" );
		}

		foreach ( var texturePath in textureBindings.Data )
		{
			// todo: How do we determine SRGB here? We should probably just fetch it from the texture itself right?
			Textures.Add( texturePath.Key, new Texture( texturePath.Value, false ) );
		}

		{
			var shaderFileBytes = FileSystem.Mounted.ReadAllBytes( "shaders/pbr.mshdr" );
			var shaderFormat = Serializer.Deserialize<MochaFile<ShaderInfo>>( shaderFileBytes );

			var boundTextures = shaderFormat.Data.Fragment.Reflection.Bindings
				.Where( binding => binding.Type == ShaderReflectionType.Texture )
				.Select( binding => (
					binding.Name,
					Textures.TryGetValue( binding.Name, out var tex ) ? tex : Texture.MissingTexture,
					binding
				) )
				.OrderBy( x => x.binding.Binding )
				.Select( x => x.Item2.NativeTexture )
				.ToList();

			NativeMaterial = new(
				Path,
				shaderFormat.Data.Vertex.Data.ToInterop(),
				shaderFormat.Data.Fragment.Data.ToInterop(),
				Vertex.VertexAttributes.ToInterop(),
				boundTextures.ToInterop(),
				SamplerType.Point,
				false
			);
		}

		//
		// alex: this might be the worst possible way to do shader hotloading.
		// perhaps we should have some sort of hook in the resource compiler
		// so that we don't have to pull this shit off
		//
		FileSystem.Mounted.CreateWatcher( "shaders", "pbr.mshdr_c", ( _ ) =>
		{
			var shaderFileBytes = FileSystem.Mounted.ReadAllBytes( "shaders/pbr.mshdr" );
			var shaderFormat = Serializer.Deserialize<MochaFile<ShaderInfo>>( shaderFileBytes );

			NativeMaterial.SetShaderData(
				shaderFormat.Data.Vertex.Data.ToInterop(),
				shaderFormat.Data.Fragment.Data.ToInterop()
			);

			NativeMaterial.Reload();
		} );
	}

	public static Material FromShader( string shaderPath, VertexAttribute[] vertexAttributes )
	{
		Material material = new();

		var shaderFileBytes = FileSystem.Mounted.ReadAllBytes( shaderPath );
		var shaderFormat = Serializer.Deserialize<MochaFile<ShaderInfo>>( shaderFileBytes );

		material.Path = "Procedural Material";

		material.NativeMaterial = new(
			material.Path,
			shaderFormat.Data.Vertex.Data.ToInterop(),
			shaderFormat.Data.Fragment.Data.ToInterop(),
			vertexAttributes.ToInterop()
		);

		return material;
	}
}
