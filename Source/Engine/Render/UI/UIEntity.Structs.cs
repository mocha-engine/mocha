using System.Runtime.InteropServices;

namespace Mocha.UI;

partial class UIEntity
{
	private UIVertex[] RectVertices => new UIVertex[]
	{
		new UIVertex { Position = new ( 0, 0, 0 ), TexCoords = new( 0, 0 ), PanelPos = new( 0, 0 ) },
		new UIVertex { Position = new ( 0, 1, 0 ), TexCoords = new( 0, 1 ), PanelPos = new( 0, 1 ) },
		new UIVertex { Position = new ( 1, 0, 0 ), TexCoords = new( 1, 0 ), PanelPos = new( 1, 0 ) },
		new UIVertex { Position = new ( 1, 1, 0 ), TexCoords = new( 1, 1 ), PanelPos = new( 1, 1 ) },
	};

	private uint[] RectIndices => new uint[] {
		2, 1, 0,
		1, 2, 3
	};

	private int RectCount = 0;
	private List<UIVertex> Vertices = new();

	[StructLayout( LayoutKind.Sequential )]
	public struct UIVertex
	{
		public Vector3 Position { get; set; }
		public Vector2 TexCoords { get; set; }
		public Vector4 Color { get; set; }
		public Vector2 PanelPos { get; set; }
		public Vector2 PanelSize { get; set; }
		public int Flags { get; set; }

		public static readonly VertexAttribute[] VertexAttributes = new[]
		{
			new VertexAttribute( "position", VertexAttributeFormat.Float3 ),
			new VertexAttribute( "texCoords", VertexAttributeFormat.Float2 ),
			new VertexAttribute( "color", VertexAttributeFormat.Float4 ),
			new VertexAttribute( "panelPos", VertexAttributeFormat.Float2 ),
			new VertexAttribute( "panelSize", VertexAttributeFormat.Float2 ),
			new VertexAttribute( "flags", VertexAttributeFormat.Int ),
		};
	}
}
