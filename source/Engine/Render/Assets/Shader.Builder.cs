using System.Text;

namespace Mocha.Renderer;

public class ShaderBuilder
{
	public static ShaderBuilder Default => new ShaderBuilder();

	public string Path { get; set; }

	internal ShaderBuilder()
	{

	}

	public ShaderBuilder FromMoyaiShader( string mshdrPath )
	{
		Path = mshdrPath;
		var shaderText = FileSystem.Game.ReadAllText( mshdrPath );

		var vertexShaderText = $"#version 450\n#define VERTEX\n{shaderText}";
		var fragmentShaderText = $"#version 450\n#define FRAGMENT\n{shaderText}";

		var vertexShaderBytes = Encoding.Default.GetBytes( vertexShaderText );
		var fragmentShaderBytes = Encoding.Default.GetBytes( fragmentShaderText );

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

		return new Shader( Path );
	}
}
