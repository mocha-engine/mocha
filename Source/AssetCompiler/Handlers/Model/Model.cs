namespace MochaTool.AssetCompiler;

public class Model
{
	public VertexInfo[] Vertices { get; set; }
	public uint[]? Indices { get; set; }
	public string Material { get; private set; }
	public bool IsIndexed { get; set; }

	public Model( VertexInfo[] vertices, uint[] indices, string material )
	{
		Vertices = vertices;
		Indices = indices;
		Material = material;
		IsIndexed = true;
	}

	public Model( VertexInfo[] vertices, string material )
	{
		Vertices = vertices;
		Material = material;
		IsIndexed = false;
	}
}
