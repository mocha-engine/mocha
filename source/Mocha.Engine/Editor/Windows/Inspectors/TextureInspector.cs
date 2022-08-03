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

		ImGuiX.Title(
			$"{FontAwesome.Image} {Path.GetFileName( texture.Path.NormalizePath() )}",
			"This is a texture."
		);

		ImGuiX.Image( texture, new Vector2( windowWidth, windowWidth ) - new Vector2( 16, 0 ) );
		ImGuiX.Separator();

		var items = new[]
		{
			( "Full Path", $"{texture.Path.NormalizePath()}" ),
			( "Size", $"{texture.Width}x{texture.Height}" ),
			( "Type", texture.Type ),
			( "Mip Levels", $"{texture.VeldridTexture.MipLevels}" ),
			( "Format", $"{texture.VeldridTexture.Format}" )
		};

		DrawProperties( $"{FontAwesome.Image} Texture", items, texture.Path );
	}
}
