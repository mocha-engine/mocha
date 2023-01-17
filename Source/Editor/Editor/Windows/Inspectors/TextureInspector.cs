namespace Mocha.Editor;

[Inspector<Texture>]
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
			ResourceType.Texture
		);

		var items = new (string, string)[]
		{
			( "Full Path", $"{texture.Path.NormalizePath()}" ),
			( "Size", $"{texture.Width}x{texture.Height}" )
		};

		DrawProperties( $"{FontAwesome.Image} Texture", items, texture.Path );

		ImGuiX.Separator();

		ImGui.SetCursorPosY( windowHeight - windowWidth - 10 );
		ImGuiX.Image( texture, new Vector2( windowWidth, windowWidth ) - new Vector2( 16, 0 ) );
	}
}
