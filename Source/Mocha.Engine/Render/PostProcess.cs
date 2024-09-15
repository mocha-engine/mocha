namespace Mocha;

public class PostProcess
{
	private Glue.SceneMesh Native => NativeEngine.GetSceneGraph().GetMesh( NativeHandle );
	private uint NativeHandle;

	private Material material;
	private Model model;

	public PostProcess( string shaderPath )
	{
		material = Material.FromShader(
			shaderPath,
			[
				new VertexAttribute( "Position", VertexAttributeFormat.Float3 ),
				new VertexAttribute( "UV", VertexAttributeFormat.Float2 )
			]
		);

		NativeHandle = NativeEngine.GetSceneGraph().CreateMesh();
		Native.SetFlags( SceneMeshFlags.PostProcess );

		model = new Model(
			[
				new Vertex() { Position = new Vector3( -1, -1, 0 ), UV = new Vector2( 0, 0 ) },
				new Vertex() { Position = new Vector3( 3, -1, 0 ), UV = new Vector2( 2, 0 ) },
				new Vertex() { Position = new Vector3( -1, 3, 0 ), UV = new Vector2( 0, 2 ) },
			],
			material
		);

		Native.SetModel( model.NativeModel );
	}
}
