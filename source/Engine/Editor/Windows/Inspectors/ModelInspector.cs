namespace Mocha.Editor;

public class ModelInspector : BaseInspector
{
	private Model model;

	public ModelInspector( Model model )
	{
		this.model = model;
	}

	public override void Draw()
	{
		ImGuiX.InspectorTitle(
			$"{Path.GetFileName( model.Path.NormalizePath() )}",
			"This is a model.",
			FileType.Model
		);

		var items = new[]
		{
			( "Full Path", $"{model.Path.NormalizePath()}" )
		};

		DrawProperties( $"{FontAwesome.Cube} Model", items, model.Path );
	}
}
