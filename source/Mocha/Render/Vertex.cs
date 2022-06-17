using System.Runtime.InteropServices;
using Veldrid;

namespace Mocha.Renderer;

[StructLayout( LayoutKind.Sequential )]
public struct Vertex
{
	public Vector3 Position { get; set; }
	public Vector3 Normal { get; set; }
	public Vector2 TexCoords { get; set; }
	public Vector3 Tangent { get; set; }
	public Vector3 Bitangent { get; set; }

	public static VertexElementDescription[] VertexElementDescriptions = new[]
	{
		new VertexElementDescription( "position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3 ),
		new VertexElementDescription( "normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3 ),
		new VertexElementDescription( "texCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2 ),
		new VertexElementDescription( "tangent", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3 ),
		new VertexElementDescription( "bitangent", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3 ),
	};
}
