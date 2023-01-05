namespace Mocha.Renderer;

public enum VertexAttributeFormat
{
	Int,
	Float,
	Float2,
	Float3,
	Float4
};

public struct VertexAttribute
{
	public string Name { get; set; }
	public VertexAttributeFormat Format { get; set; }

	public VertexAttribute( string name, VertexAttributeFormat format )
	{
		Name = name;
		Format = format;
	}
}
