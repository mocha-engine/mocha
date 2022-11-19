using System.Runtime.InteropServices;

namespace Mocha.Renderer;

[Icon( FontAwesome.Cube ), Title( "Model" )]
public class Model : Asset
{
	private Glue.ManagedModel NativeModel { get; set; }

	public Material Material { get; set; }
	public bool IsIndexed { get; private set; }

	private int indexCount;

	public Model( string path, Material material, bool isIndexed )
	{
		Path = path;
		Material = material;
		IsIndexed = isIndexed;

		All.Add( this );

		Material.Shader.OnRecompile += CreateResources;
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

	private void SetupMesh( Vertex[] vertices )
	{
		NativeModel = new();

		int strideInBytes = Marshal.SizeOf( typeof( Vertex ) );
		int sizeInBytes = strideInBytes * vertices.Length;

		unsafe
		{
			fixed ( void* data = vertices )
			{
				NativeModel.SetVertexData( sizeInBytes, (IntPtr)data );
			}
		}
	}

	private void SetupMesh( Vertex[] vertices, uint[] indices )
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

	private void CreateResources()
	{
		NativeModel.Finish();
	}

	public void Render()
	{
		// Log.Trace( "Render" );
	}
}
