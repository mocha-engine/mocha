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
		EditorHelpers.Title(
			$"{FontAwesome.Glasses} {Path.GetFileName( texture.Path.NormalizePath() )}",
			"This is a shader."
		);

		var items = new[]
		{
			( "Full Path", $"{texture.Path.NormalizePath()}" )
		};

		EditorHelpers.TextBold( $"{FontAwesome.Glasses} Shader" );

		DrawTable( items );

		EditorHelpers.Separator();

		DrawButtons( Path.GetFullPath( texture.Path ) );
	}
}
