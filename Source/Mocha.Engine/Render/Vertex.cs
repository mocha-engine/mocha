using System.Runtime.InteropServices;

namespace Mocha;

[StructLayout( LayoutKind.Sequential )]
public struct Vertex
{
	public Vector3 Position { get; set; }
	public Vector3 Normal { get; set; }
	public Vector3 Color { get; set; }
	public Vector2 UV { get; set; }

	public Vector3 Tangent { get; set; }
	public Vector3 Bitangent { get; set; }

	public static VertexAttribute[] VertexAttributes = new[]
	{
		new VertexAttribute( "position", VertexAttributeFormat.Float3 ),
		new VertexAttribute( "normal", VertexAttributeFormat.Float3 ),
		new VertexAttribute( "color", VertexAttributeFormat.Float3 ),
		new VertexAttribute( "texCoords", VertexAttributeFormat.Float2 ),
		new VertexAttribute( "tangent", VertexAttributeFormat.Float3 ),
		new VertexAttribute( "bitangent", VertexAttributeFormat.Float3 ),
	};
}
