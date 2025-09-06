namespace Mocha.Editor;

[Inspector<Texture>]
public class TextureInspector : BaseInspector
{
	private Texture _texture;

	public TextureInspector( Texture texture )
	{
		this._texture = texture;
	}

	public override void Draw()
	{
		var (windowWidth, windowHeight) = (ImGui.GetWindowWidth(), ImGui.GetWindowHeight());

		ImGuiX.InspectorTitle(
			$"{Path.GetFileName( _texture.Path.NormalizePath() )}",
			"This is a texture.",
			ResourceType.Texture
		);

		var items = new (string, string)[]
		{
			( "Full Path", $"{_texture.Path.NormalizePath()}" ),
			( "Size", $"{_texture.Width}x{_texture.Height}" )
		};

		DrawProperties( $"{FontAwesome.Image} Texture", items, _texture.Path );

		ImGuiX.Separator();

		ImGui.SetCursorPosY( windowHeight - windowWidth - 10 );
		ImGuiX.Image( _texture, new Vector2( windowWidth, windowWidth ) - new Vector2( 16, 0 ) );
	}
}
