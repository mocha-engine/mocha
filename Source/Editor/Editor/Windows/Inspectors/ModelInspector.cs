namespace Mocha.Editor;

[Inspector<Model>]
public class ModelInspector : BaseInspector
{
	private readonly Model model;

	public ModelInspector( Model model )
	{
		this.model = model;
	}

	public override void Draw()
	{
		ImGuiX.InspectorTitle(
			$"{Path.GetFileName( model.Path.NormalizePath() )}",
			"This is a model.",
			ResourceType.Model
		);

		var items = new[]
		{
			( "Full Path", $"{model.Path.NormalizePath()}" )
		};

		DrawProperties( $"{FontAwesome.Cube} Model", items, model.Path );
	}
}
