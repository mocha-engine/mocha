namespace Mocha.Renderer;

public partial class ProceduralMeshes
{
	internal static class Plane
	{
		public static Vertex[] Vertices = new[]
		{
			new Vertex()
			{
				Position = new Vector3(-1.0f, -1.0f, 0.0f),
				UV = new Vector2(0.0f, 0.0f),
				Color = new Vector3( 1, 0, 1 ),
				Normal = new Vector3(0.0f, 0.0f, 1.0f),
			},
			new Vertex()
			{
				Position = new Vector3(1.0f, -1.0f, 0.0f),
				UV = new Vector2(1.0f, 0.0f),
				Color = new Vector3( 1, 0, 1 ),
				Normal = new Vector3(0.0f, 0.0f, 1.0f),
			},
			new Vertex()
			{
				Position = new Vector3(-1.0f, 1.0f, 0.0f),
				UV = new Vector2(0.0f, 1.0f),
				Color = new Vector3( 1, 0, 1 ),
				Normal = new Vector3(0.0f, 0.0f, 1.0f),
			},
			new Vertex()
			{
				Position = new Vector3(1.0f, 1.0f, 0.0f),
				UV = new Vector2(1.0f, 1.0f),
				Color = new Vector3( 1, 0, 1 ),
				Normal = new Vector3(0.0f, 0.0f, 1.0f),
			}
		};

		public static uint[] Indices { get; } = new uint[]
		{
			0, 2, 3,
			3, 1, 0,
		};

		public static Model GenerateModel( Material material )
		{
			return new Model( Vertices, Indices, material );
		}
	}
}
