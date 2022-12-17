namespace Mocha.Renderer;

public partial class Model : Model<Vertex>
{
	/// <summary>
	/// Loads a model from an MMDL (compiled) file.
	/// </summary>
	public Model( string path )
	{
		All.Add( this );

		LoadFromPath( path );
	}

	/// <summary>
	/// Creates an indexed model from a given set of vertices and indices.
	/// </summary>
	public Model( string path, Vertex[] vertices, uint[] indices, Material material ) : this( path )
	{
		AddMesh( vertices, indices, material );
	}

	/// <summary>
	/// Creates a basic model from a given set of vertices.
	/// </summary>
	public Model( string path, Vertex[] vertices, Material material ) : this( path )
	{
		AddMesh( vertices, material );
	}
}

[Icon( FontAwesome.Cube ), Title( "Model" )]
public partial class Model<T> : Asset, IModel where T : struct
{
	public Glue.Model NativeModel { get; set; }

	public Model()
	{
		NativeModel = new();
	}

	protected void AddMesh( T[] vertices, Material material )
	{
		NativeModel.AddMesh( vertices.ToInterop(), new uint[0].ToInterop(), material.NativeMaterial );
	}

	protected void AddMesh( T[] vertices, uint[] indices, Material material )
	{
		NativeModel.AddMesh( vertices.ToInterop(), indices.ToInterop(), material.NativeMaterial );
	}
}

public interface IModel
{
	Glue.Model NativeModel { get; set; }
}
