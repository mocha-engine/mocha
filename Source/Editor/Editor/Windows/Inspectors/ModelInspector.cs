namespace Mocha.Editor;

[Inspector<Model>]
public class ModelInspector : BaseInspector
{
	private readonly Model _model;

	public ModelInspector( Model model )
	{
		this._model = model;
	}

	public override void Draw()
	{
		ImGuiX.InspectorTitle(
			$"{Path.GetFileName( _model.Path.NormalizePath() )}",
			"This is a model.",
			ResourceType.Model
		);

		var items = new[]
		{
			( "Full Path", $"{_model.Path.NormalizePath()}" )
		};

		DrawProperties( $"{FontAwesome.Cube} Model", items, _model.Path );
	}
}
