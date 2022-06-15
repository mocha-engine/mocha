namespace Mocha;

public class Model
{
	public Vertex[] Vertices { get; set; }
	public uint[] Indices { get; set; }
	public Material Material { get; private set; }
	public bool IsIndexed { get; set; }

	public Model( Vertex[] vertices, uint[] indices, Material material )
	{
		Vertices = vertices;
		Indices = indices;
		Material = material;
		IsIndexed = true;
	}

	public Model( Vertex[] vertices, Material material )
	{
		Vertices = vertices;
		Material = material;
		IsIndexed = false;
	}
}
