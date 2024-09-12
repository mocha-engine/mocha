namespace Mocha;

[Icon( FontAwesome.FaceGrinStars ), Title( "Material" )]
public class Material : Asset
{
	public Texture? DiffuseTexture { get; set; } = Texture.MissingTexture;
	public Texture? NormalTexture { get; set; } = Texture.Normal;
	public Texture? AmbientOcclusionTexture { get; set; } = Texture.One;
	public Texture? MetalnessTexture { get; set; } = Texture.Zero;
	public Texture? RoughnessTexture { get; set; } = Texture.One;

	public static Texture BlueNoiseTexture { get; } = new Texture( "textures/bluenoise.mtex", false );

	public Glue.Material NativeMaterial { get; private set; }

	/// <summary>
	/// Loads a material from an MMAT (compiled) file.
	/// </summary>
	public Material( string path )
	{
		Path = path;

		MochaFile<MaterialInfo> materialFormat = new();

		if ( FileSystem.Mounted.Exists( path ) )
		{
			var fileBytes = FileSystem.Mounted.ReadAllBytes( path );
			materialFormat = Serializer.Deserialize<MochaFile<MaterialInfo>>( fileBytes );
		}
		else
		{
			Log.Warning( $"Material '{path}' does not exist" );
		}

		if ( !string.IsNullOrEmpty( materialFormat.Data.DiffuseTexture ) )
			DiffuseTexture = new Texture( materialFormat.Data.DiffuseTexture );

		if ( !string.IsNullOrEmpty( materialFormat.Data.NormalTexture ) )
			NormalTexture = new Texture( materialFormat.Data.NormalTexture, false );

		if ( !string.IsNullOrEmpty( materialFormat.Data.AmbientOcclusionTexture ) )
			AmbientOcclusionTexture = new Texture( materialFormat.Data.AmbientOcclusionTexture, false );

		if ( !string.IsNullOrEmpty( materialFormat.Data.MetalnessTexture ) )
			MetalnessTexture = new Texture( materialFormat.Data.MetalnessTexture, false );

		if ( !string.IsNullOrEmpty( materialFormat.Data.RoughnessTexture ) )
			RoughnessTexture = new Texture( materialFormat.Data.RoughnessTexture, false );

		var textures = new List<Glue.Texture>()
		{
			DiffuseTexture.NativeTexture,
			NormalTexture.NativeTexture,
			AmbientOcclusionTexture.NativeTexture,
			MetalnessTexture.NativeTexture,
			RoughnessTexture.NativeTexture,
			BlueNoiseTexture.NativeTexture
		};

		{
			var shaderFileBytes = FileSystem.Mounted.ReadAllBytes( "shaders/pbr.mshdr" );
			var shaderFormat = Serializer.Deserialize<MochaFile<ShaderInfo>>( shaderFileBytes );

			NativeMaterial = new(
				Path,
				shaderFormat.Data.VertexShaderData.ToInterop(),
				shaderFormat.Data.FragmentShaderData.ToInterop(),
				Vertex.VertexAttributes.ToInterop(),
				textures.ToInterop(),
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
				shaderFormat.Data.VertexShaderData.ToInterop(),
				shaderFormat.Data.FragmentShaderData.ToInterop()
			);

			NativeMaterial.Reload();
		} );
	}

	/// <summary>
	/// Creates a material.
	/// </summary>
	public Material( string shaderPath, VertexAttribute[] vertexAttributes, Texture? diffuseTexture = null,
		Texture? normalTexture = null, Texture? ambientOcclusionTexture = null, Texture? metalnessTexture = null,
		Texture? roughnessTexture = null, SamplerType sampler = SamplerType.Point, bool ignoreDepth = false )
	{
		var shaderFileBytes = FileSystem.Mounted.ReadAllBytes( shaderPath );
		var shaderFormat = Serializer.Deserialize<MochaFile<ShaderInfo>>( shaderFileBytes );

		Path = "Procedural Material";

		DiffuseTexture = diffuseTexture ?? Texture.MissingTexture;
		NormalTexture = normalTexture ?? Texture.Normal;
		AmbientOcclusionTexture = ambientOcclusionTexture ?? Texture.One;
		MetalnessTexture = metalnessTexture ?? Texture.Zero;
		RoughnessTexture = roughnessTexture ?? Texture.One;

		var textures = new List<Glue.Texture>()
		{
			DiffuseTexture.NativeTexture,
			NormalTexture.NativeTexture,
			AmbientOcclusionTexture.NativeTexture,
			MetalnessTexture.NativeTexture,
			RoughnessTexture.NativeTexture
		};

		NativeMaterial = new(
			Path,
			shaderFormat.Data.VertexShaderData.ToInterop(),
			shaderFormat.Data.FragmentShaderData.ToInterop(),
			vertexAttributes.ToInterop(),
			textures.ToInterop(),
			sampler,
			ignoreDepth
		);

		// TODO: File watcher here!
	}
}
