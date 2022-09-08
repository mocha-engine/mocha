using System.Text;
using Veldrid.SPIRV;

namespace Mocha.Renderer;

public class ShaderBuilder
{
	private ShaderDescription VertexShaderDescription;
	private ShaderDescription FragmentShaderDescription;

	private Framebuffer targetFramebuffer = SceneWorld.Current.Camera.Framebuffer;
	private FaceCullMode faceCullMode = FaceCullMode.Back;

	public static ShaderBuilder Default => new ShaderBuilder();

	public string Path { get; set; }

	internal ShaderBuilder()
	{

	}

	public ShaderBuilder FromPath( string mshdrPath )
	{
		Path = mshdrPath;
		var shaderText = FileSystem.Game.ReadAllText( mshdrPath );

		var vertexShaderText = $"#version 450\n#define VERTEX\n{shaderText}";
		var fragmentShaderText = $"#version 450\n#define FRAGMENT\n{shaderText}";

		var vertexShaderBytes = Encoding.Default.GetBytes( vertexShaderText );
		var fragmentShaderBytes = Encoding.Default.GetBytes( fragmentShaderText );

		VertexShaderDescription = new ShaderDescription( ShaderStages.Vertex, vertexShaderBytes, "main" );
		FragmentShaderDescription = new ShaderDescription( ShaderStages.Fragment, fragmentShaderBytes, "main" );

		return this;
	}

	public ShaderBuilder WithFramebuffer( Framebuffer framebuffer )
	{
		this.targetFramebuffer = framebuffer;
		return this;
	}

	public ShaderBuilder WithFaceCullMode( FaceCullMode faceCullMode )
	{
		this.faceCullMode = faceCullMode;
		return this;
	}

	public Shader Build()
	{
		if ( Asset.All.OfType<Shader>().Any( x => x.Path == Path ) )
		{
			Log.Trace( $"Using cached shader {Path}" );
			return Asset.All.OfType<Shader>().First( x => x.Path == Path );
		}

		Log.Trace( $"Compiling shader {Path}" );
		try
		{
			var fragCompilation = SpirvCompilation.CompileGlslToSpirv(
				Encoding.UTF8.GetString( FragmentShaderDescription.ShaderBytes ),
				Path + "_FS",
				ShaderStages.Fragment,
				new GlslCompileOptions( debug: false ) );
			FragmentShaderDescription.ShaderBytes = fragCompilation.SpirvBytes;

			var vertCompilation = SpirvCompilation.CompileGlslToSpirv(
				Encoding.UTF8.GetString( VertexShaderDescription.ShaderBytes ),
				Path + "_VS",
				ShaderStages.Vertex,
				new GlslCompileOptions( debug: false ) );
			VertexShaderDescription.ShaderBytes = vertCompilation.SpirvBytes;

			var shaderProgram = Device.ResourceFactory.CreateFromSpirv( VertexShaderDescription, FragmentShaderDescription );
			return new Shader( Path, targetFramebuffer, faceCullMode, shaderProgram );
		}
		catch ( Exception ex )
		{
			Log.Error( ex.ToString() );
			return default;
		}
	}
}
