using SharpDX.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Mocha
{
	public class ShaderBuilder
	{
		private ShaderDescription VertexShaderDescription;
		private ShaderDescription FragmentShaderDescription;

		public static ShaderBuilder Default => new ShaderBuilder();

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
			FragmentShaderDescription = CreateShaderDescription( fragPath, ShaderStages.Fragment );
			return this;
		}

		public ShaderBuilder WithVertex( string vertPath )
		{
			VertexShaderDescription = CreateShaderDescription( vertPath, ShaderStages.Vertex );
			return this;
		}

		public Shader Build()
		{
			try
			{
				var fragCompilation = SpirvCompilation.CompileGlslToSpirv(
					Encoding.UTF8.GetString( FragmentShaderDescription.ShaderBytes ),
					"FS",
					ShaderStages.Fragment,
					new GlslCompileOptions( debug: true ) );
				FragmentShaderDescription.ShaderBytes = fragCompilation.SpirvBytes;

				var vertCompilation = SpirvCompilation.CompileGlslToSpirv(
					Encoding.UTF8.GetString( VertexShaderDescription.ShaderBytes ),
					"VS",
					ShaderStages.Vertex,
					new GlslCompileOptions( debug: true ) );
				VertexShaderDescription.ShaderBytes = vertCompilation.SpirvBytes;

				var shaderProgram = Device.ResourceFactory.CreateFromSpirv( VertexShaderDescription, FragmentShaderDescription );
				return new Shader( shaderProgram );
			}
			catch ( Exception ex )
			{
				Log.Error( ex.ToString() );
				return default;
			}
		}
	}
}
