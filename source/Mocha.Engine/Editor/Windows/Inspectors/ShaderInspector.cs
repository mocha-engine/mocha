namespace Mocha.Engine;

public class ShaderInspector : BaseInspector
{
	private Shader texture;

	public ShaderInspector( Shader texture )
	{
		this.texture = texture;
	}

	public override void Draw()
	{
		ImGuiX.InspectorTitle(
			$"{Path.GetFileName( texture.Path.NormalizePath() )}",
			"This is a shader.",
			FileType.Shader
		);

		var items = new[]
		{
			( "Full Path", $"{texture.Path.NormalizePath()}" )
		};

		DrawProperties( $"{FontAwesome.Glasses} Shader", items, texture.Path );
	}
}
