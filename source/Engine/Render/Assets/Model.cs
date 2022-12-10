using System.Runtime.InteropServices;

namespace Mocha.Renderer;

public partial class Model : Model<Vertex>
{
	public Model( string path )
	{
		LoadFromPath( path );
	}

	private Model( string path, Material material, bool isIndexed )
	{
		Path = path;
		Material = material;
		IsIndexed = isIndexed;

		All.Add( this );
	}

	public Model( string path, Vertex[] vertices, uint[] indices, Material material ) : this( path, material, true )
	{
		SetupMesh( vertices, indices );
		CreateResources();
	}

	public Model( string path, Vertex[] vertices, Material material ) : this( path, material, false )
	{
		SetupMesh( vertices );
		CreateResources();
	}
}

[Icon( FontAwesome.Cube ), Title( "Model" )]
public partial class Model<T> : Asset
	where T : struct
{
	public Glue.ManagedModel NativeModel { get; set; }

	public Material Material { get; set; }
	public bool IsIndexed { get; protected set; }

	private int indexCount;

	protected void SetupMesh( T[] vertices )
	{
		NativeModel = new();

		int strideInBytes = Marshal.SizeOf( typeof( T ) );
		int sizeInBytes = strideInBytes * vertices.Length;

		unsafe
		{
			fixed ( void* data = vertices )
			{
				NativeModel.SetVertexData( sizeInBytes, (IntPtr)data );
			}
		}
	}

	protected void SetupMesh( T[] vertices, uint[] indices )
	{
		SetupMesh( vertices );

		int strideInBytes = Marshal.SizeOf( typeof( uint ) );
		int sizeInBytes = strideInBytes * indices.Length;

		unsafe
		{
			fixed ( void* data = indices )
			{
				NativeModel.SetIndexData( sizeInBytes, (IntPtr)data );
			}
		}
	}

	protected void CreateResources()
	{
		NativeModel.Finish( Material.DiffuseTexture.NativeTexture.NativePtr );
	}

	public void SetIndices( uint[] indices )
	{
		throw new NotImplementedException();
	}

	public void SetVertices( T[] vertices )
	{
		throw new NotImplementedException();
	}
}
