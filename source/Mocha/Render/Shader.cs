namespace Mocha;

public class Shader
{
	public static List<Shader> All = new();
	public Veldrid.Shader[] ShaderProgram { get; }
	public string Path { get; set; }

	internal Shader( string path, Veldrid.Shader[] shaderProgram )
	{
		ShaderProgram = shaderProgram;
		Path = path;

		All.Add( this );
	}
}
