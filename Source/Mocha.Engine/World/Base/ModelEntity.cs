namespace Mocha;

[Category( "World" ), Title( "Static Mesh" ), Icon( FontAwesome.Cube )]
public partial class StaticMeshActor : Actor
{
	[HideInInspector]
	private Glue.SceneMesh Native => NativeEngine.GetSceneGraph().GetMesh( NativeHandle );

	private uint NativeHandle;

	private string _modelPath;

	[Category( "Rendering" )]
	public IModel Model
	{
		set
		{
			Native.SetModel( value.NativeModel );
			_modelPath = value.Path;
		}
	}

	public StaticMeshActor() : base()
	{
		NativeHandle = NativeEngine.GetSceneGraph().CreateMesh();
	}

	public StaticMeshActor( string path ) : this()
	{
		Model = new Model( path );
	}
}
