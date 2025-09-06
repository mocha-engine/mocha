namespace Mocha;

partial class ProceduralMeshes
{
	public static class Cube
	{
		private static float[] s_vertices = new[] {
            // Top
            -.5f,.5f,-.5f,     0,1,0,     0, 0,
			.5f,.5f,-.5f,      0,1,0,     1, 0,
			.5f,.5f,.5f,       0,1,0,     1, 1,
			-.5f,.5f,.5f,      0,1,0,     0, 1,

            // Bottom                                                             
            -.5f,-.5f,.5f,     0,-1,0,     0, 0,
			.5f,-.5f,.5f,      0,-1,0,     1, 0,
			.5f,-.5f,-.5f,     0,-1,0,     1, 1,
			-.5f,-.5f,-.5f,    0,-1,0,     0, 1,

            // Left                                                               
            -.5f,.5f,-.5f,     -1,0,0,    0, 0,
			-.5f,.5f,.5f,      -1,0,0,    1, 0,
			-.5f,-.5f,.5f,     -1,0,0,    1, 1,
			-.5f,-.5f,-.5f,    -1,0,0,    0, 1,

            // Right                                                              
            .5f,.5f,.5f,       1,0,0,     0, 0,
			.5f,.5f,-.5f,      1,0,0,     1, 0,
			.5f,-.5f,-.5f,     1,0,0,     1, 1,
			.5f,-.5f,.5f,      1,0,0,     0, 1,

            // Back                                                               
            .5f,.5f,-.5f,      0,0,-1,    0, 0,
			-.5f,.5f,-.5f,     0,0,-1,    1, 0,
			-.5f,-.5f,-.5f,    0,0,-1,    1, 1,
			.5f,-.5f,-.5f,     0,0,-1,    0, 1,

            // Front                                                              
            -.5f,.5f,.5f,      0,0,1,     0, 0,
			.5f,.5f,.5f,       0,0,1,     1, 0,
			.5f,-.5f,.5f,      0,0,1,     1, 1,
			-.5f,-.5f,.5f,     0,0,1,     0, 1,
		};


		private static uint[] s_indices =
		{
			0,1,2, 0,2,3,
			4,5,6, 4,6,7,
			8,9,10, 8,10,11,
			12,13,14, 12,14,15,
			16,17,18, 16,18,19,
			20,21,22, 20,22,23,
		};

		public static List<Vertex> Vertices
		{
			get
			{
				List<Vertex> tmp = new List<Vertex>();

				for ( int i = 0; i < s_vertices.Length; i += 8 )
				{
					var x = s_vertices[i];
					var y = s_vertices[i + 1];
					var z = s_vertices[i + 2];

					var nX = s_vertices[i + 3];
					var nY = s_vertices[i + 4];
					var nZ = s_vertices[i + 5];

					var u = s_vertices[i + 6];
					var v = s_vertices[i + 7];

					tmp.Add( new Vertex()
					{
						Position = new Vector3( x, y, z ),
						UV = new Vector2( u, v ),
						Color = new Vector3( x, y, z ),
						Normal = new Vector3( nX, nY, nZ ),
					} );
				}

				return tmp;
			}
		}

		public static Model GenerateModel( Material material )
		{
			return new Model( Vertices.ToArray(), s_indices, material );
		}
	}
}
