using ImGuiNET;

namespace Mocha.Engine;

public class TextureInspector : BaseInspector
{
	private Texture texture;

	public TextureInspector( Texture texture )
	{
		this.texture = texture;
	}

	public override void Draw()
	{
		var windowWidth = ImGui.GetWindowWidth();

		EditorHelpers.Title(
			$"{FontAwesome.Image} {Path.GetFileName( texture.Path.NormalizePath() )}",
			"This is a texture."
		);

		EditorHelpers.Image( texture, new Vector2( windowWidth, windowWidth ) - new Vector2( 16, 0 ) );
		EditorHelpers.Separator();

		var items = new[]
		{
			( "Full Path", $"{texture.Path.NormalizePath()}" ),
			( "Size", $"{texture.Width}x{texture.Height}" ),
			( "Type", texture.Type ),
			( "Mip Levels", $"{texture.VeldridTexture.MipLevels}" ),
			( "Format", $"{texture.VeldridTexture.Format}" )
		};

		EditorHelpers.TextBold( $"{FontAwesome.Image} Texture" );

		DrawTable( items );

		EditorHelpers.Separator();

		DrawButtons( Path.GetFullPath( texture.Path ) );
	}
}
