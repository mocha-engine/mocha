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

		{
			var shaderFileBytes = FileSystem.Mounted.ReadAllBytes( "shaders/pbr.mshdr" );
			var shaderFormat = Serializer.Deserialize<MochaFile<ShaderInfo>>( shaderFileBytes );

			// resolve bindings
			var requiredBindings = shaderFormat.Data.Fragment.Reflection.Bindings
				.Where( binding => binding.Type == ShaderReflectionType.Texture )
				.ToList();

			// load textures
			foreach ( var binding in requiredBindings )
			{
				if ( !Textures.ContainsKey( binding.Name ) && textureBindings.Data.TryGetValue( binding.Name, out string? value ) )
				{
					bool isSrgb = binding.Attributes.Any( a => a.Type == ShaderReflectionAttributeType.SrgbRead );

					Textures.Add( binding.Name, new Texture( value, isSrgb ) );
				}
			}

			// create the ordered list of bound textures
			var boundTextures = requiredBindings
				.OrderBy( x => x.Binding )
				.Select( binding =>
				{
					if ( Textures.TryGetValue( binding.Name, out var tex ) )
						return tex;

					// check for default attribute
					var defaultAttr = binding.Attributes.Where( a => a.Type == ShaderReflectionAttributeType.Default ).Any()
						? binding.Attributes.First( a => a.Type == ShaderReflectionAttributeType.Default )
						: default;

					bool isSrgb = binding.Attributes.Any( a => a.Type == ShaderReflectionAttributeType.SrgbRead );

					if ( defaultAttr.Type == ShaderReflectionAttributeType.Default )
					{
						var data = defaultAttr.GetData<DefaultAttributeData>();
						return new Texture( data.ValueR, data.ValueG, data.ValueB, data.ValueA, isSrgb );
					}

					return Texture.MissingTexture;
				} )
				.Select( x => x.NativeTexture )
				.ToList();

			NativeMaterial = new(
				Path,
				shaderFormat.Data.Vertex.Data.ToInterop(),
				shaderFormat.Data.Fragment.Data.ToInterop(),
				Vertex.VertexAttributes.ToInterop(),
				boundTextures.ToInterop(),
				SamplerType.Anisotropic,
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
