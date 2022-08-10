namespace Mocha.Renderer;

public class ShaderBuilder
{
	public static ShaderBuilder Default => new ShaderBuilder();

	private string Source { get; set; }
	public string Path { get; set; }

	internal ShaderBuilder()
	{

	}

	public ShaderBuilder FromMoyaiShader( string mshdrPath )
	{
		Path = mshdrPath;
		Source = FileSystem.Game.ReadAllText( mshdrPath );

		return this;
	}

	public Shader Build()
	{
		if ( Asset.All.OfType<Shader>().Any( x => x.Path == Path ) )
		{
			Log.Trace( $"Using cached shader {Path}" );
			return Asset.All.OfType<Shader>().First( x => x.Path == Path );
		}

		return new Shader( Path, Source );
	}
}
