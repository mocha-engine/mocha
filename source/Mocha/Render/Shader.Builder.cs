using SharpDX.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Mocha.Renderer;

public class ShaderBuilder
{
	private ShaderDescription VertexShaderDescription;
	private ShaderDescription FragmentShaderDescription;

	public static ShaderBuilder Default => new ShaderBuilder();

	public string Path { get; set; }

	internal ShaderBuilder()
	{

	}

	private ShaderDescription CreateShaderDescription( string path, ShaderStages shaderStage )
	{
		var shaderBytes = File.ReadAllBytes( path );
		var shaderDescription = new ShaderDescription( shaderStage, shaderBytes, "main" );

		return shaderDescription;
	}

	public ShaderBuilder FromMoyaiShader( string mshdrPath )
	{
		Path = mshdrPath;
		var shaderText = File.ReadAllText( mshdrPath );

		var vertexShaderText = $"#version 450\n#define VERTEX\n{shaderText}";
		var fragmentShaderText = $"#version 450\n#define FRAGMENT\n{shaderText}";

		var vertexShaderBytes = Encoding.Default.GetBytes( vertexShaderText );
		var fragmentShaderBytes = Encoding.Default.GetBytes( fragmentShaderText );

		VertexShaderDescription = new ShaderDescription( ShaderStages.Vertex, vertexShaderBytes, "main" );
		FragmentShaderDescription = new ShaderDescription( ShaderStages.Fragment, fragmentShaderBytes, "main" );

		return this;
	}

	public ShaderBuilder WithFragment( string fragPath )
	{
		Path += $"{fragPath};";
		FragmentShaderDescription = CreateShaderDescription( fragPath, ShaderStages.Fragment );
		return this;
	}

	public ShaderBuilder WithVertex( string vertPath )
	{
		Path += $"{vertPath};";
		VertexShaderDescription = CreateShaderDescription( vertPath, ShaderStages.Vertex );
		return this;
	}

	public Shader Build()
	{
		if ( Shader.All.Any( x => x.Path == Path ) )
		{
			Log.Trace( $"Using cached shader {Path}" );
			return Shader.All.First( x => x.Path == Path );
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
			return new Shader( Path, shaderProgram );
		}
		catch ( Exception ex )
		{
			Log.Error( ex.ToString() );
			return default;
		}
	}
}
