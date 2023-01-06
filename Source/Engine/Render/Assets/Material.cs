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

	public static Texture BlueNoiseTexture { get; } = new Texture( "core/textures/bluenoise.mtex" );

	public Glue.Material NativeMaterial { get; private set; }

	private FileSystemWatcher Watcher { get; set; }

	/// <summary>
	/// Loads a material from an MMAT (compiled) file.
	/// </summary>
	public Material( string path )
	{
		Path = path;

		// TODO: Hook up to filesystem
		const string DefaultShaderPath = "content/core/shaders/pbr.mshdr";

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

		var textures = new List<Glue.Texture>()
		{
			DiffuseTexture.NativeTexture,
			NormalTexture.NativeTexture,
			AmbientOcclusionTexture.NativeTexture,
			MetalnessTexture.NativeTexture,
			RoughnessTexture.NativeTexture,
			BlueNoiseTexture.NativeTexture
		};

		NativeMaterial = new(
			DefaultShaderPath,
			Vertex.VertexAttributes.ToInterop(),
			textures.ToInterop(),
			SamplerType.Point,
			false
		);

		Watcher = FileSystem.Game.CreateWatcher( "core/shaders", "*.*", _ => NativeMaterial.ReloadShaders() );
	}

	/// <summary>
	/// Creates a material.
	/// </summary>
	public Material( string shaderPath, VertexAttribute[] vertexAttributes, Texture? diffuseTexture = null,
		Texture? normalTexture = null, Texture? ambientOcclusionTexture = null, Texture? metalnessTexture = null,
		Texture? roughnessTexture = null, SamplerType sampler = SamplerType.Point, bool ignoreDepth = false )
	{
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

		Watcher = FileSystem.Game.CreateWatcher( "core/shaders", "*.*", _ =>
		{
			NativeMaterial.ReloadShaders();
		} );

		NativeMaterial = new(
			shaderPath,
			vertexAttributes.ToInterop(),
			textures.ToInterop(),
			sampler,
			ignoreDepth
		);
	}
}
