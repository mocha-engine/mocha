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
		var (windowWidth, windowHeight) = (ImGui.GetWindowWidth(), ImGui.GetWindowHeight());

		ImGuiX.InspectorTitle(
			$"{Path.GetFileName( texture.Path.NormalizePath() )}",
			"This is a texture.",
			FileType.Texture
		);

		var items = new[]
		{
			( "Full Path", $"{texture.Path.NormalizePath()}" ),
			( "Size", $"{texture.Width}x{texture.Height}" ),
			( "Type", texture.Type ),
			( "Mip Levels", $"{texture.VeldridTexture.MipLevels}" ),
			( "Format", $"{texture.VeldridTexture.Format}" )
		};

		DrawProperties( $"{FontAwesome.Image} Texture", items, texture.Path );

		ImGuiX.Separator();

		ImGui.SetCursorPosY( windowHeight - windowWidth - 10 );
		ImGuiX.Image( texture, new Vector2( windowWidth, windowWidth ) - new Vector2( 16, 0 ) );
	}
}
