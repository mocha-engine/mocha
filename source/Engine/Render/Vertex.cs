using System.Runtime.InteropServices;

namespace Mocha.Renderer;

[StructLayout( LayoutKind.Sequential )]
public struct Vertex
{
	public Vector3 Position { get; set; }
	public Vector3 Normal { get; set; }
	public Vector3 Color { get; set; }
	// public Vector2 TexCoords { get; set; }
	// public Vector3 Tangent { get; set; }
	// public Vector3 Bitangent { get; set; }
}
