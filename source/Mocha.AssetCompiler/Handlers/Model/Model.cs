namespace Mocha.AssetCompiler;

public class Model
{
	public VertexInfo[] Vertices { get; set; }
	public uint[] Indices { get; set; }
	public Material Material { get; private set; }
	public bool IsIndexed { get; set; }

	public Model( VertexInfo[] vertices, uint[] indices, Material material )
	{
		Vertices = vertices;
		Indices = indices;
		Material = material;
		IsIndexed = true;
	}

	public Model( VertexInfo[] vertices, Material material )
	{
		Vertices = vertices;
		Material = material;
		IsIndexed = false;
	}
}
