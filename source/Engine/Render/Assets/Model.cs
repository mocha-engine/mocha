using System.Runtime.InteropServices;

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
public partial class Model<T> : Asset where T : struct
{
	public Glue.ManagedModel NativeModel { get; set; }

	public Model()
	{
		NativeModel = new();
	}

	protected void AddMesh( T[] vertices, Material material )
	{
		unsafe
		{
			int vertexStride = Marshal.SizeOf( typeof( T ) );
			int vertexSize = vertexStride * vertices.Length;

			fixed ( void* vertexData = vertices )
			{
				NativeModel.AddMesh( vertices.Length, vertexSize, (IntPtr)vertexData, 0, 0, IntPtr.Zero, material.NativeMaterial.NativePtr );
			}
		}
	}

	protected void AddMesh( T[] vertices, uint[] indices, Material material )
	{
		unsafe
		{
			int vertexStride = Marshal.SizeOf( typeof( T ) );
			int vertexSize = vertexStride * vertices.Length;

			int indexStride = Marshal.SizeOf( typeof( uint ) );
			int indexSize = indexStride * indices.Length;

			fixed ( void* vertexData = vertices )
			fixed ( void* indexData = indices )
			{
				NativeModel.AddMesh( vertices.Length, vertexSize, (IntPtr)vertexData, indices.Length, indexSize, (IntPtr)indexData, material.NativeMaterial.NativePtr );
			}
		}
	}
}
