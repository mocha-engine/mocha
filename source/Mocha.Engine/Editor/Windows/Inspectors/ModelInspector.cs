namespace Mocha.Engine;

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
			( "Full Path", $"{model.Path.NormalizePath()}" ),
			( "Vertex Buffer", $"{MathX.ToSize( model.VertexBuffer?.SizeInBytes ?? 0, MathX.SizeUnits.KB )}" ),
			( "Index Buffer", $"{MathX.ToSize( model.IndexBuffer?.SizeInBytes ?? 0, MathX.SizeUnits.KB )}" ),
			( "TBN Buffer", $"{MathX.ToSize( model.TBNBuffer?.SizeInBytes ?? 0, MathX.SizeUnits.KB )}" ),
			( "Uses indices?", $"{model.IsIndexed}" ),
			( "Material", $"{model.Material.Path}" )
		};

		DrawProperties( $"{FontAwesome.Cube} Model", items, model.Path );
	}
}
