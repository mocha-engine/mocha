using System.Runtime.InteropServices;

namespace Mocha.Renderer;

[Icon( FontAwesome.Cube ), Title( "Model" )]
public class Model : Asset
{
	private Glue.CDeviceBuffer VertexBuffer { get; set; }
	private Glue.CDeviceBuffer IndexBuffer { get; set; }

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
		CreateUniformBuffer();
		CreateResources();
	}

	public Model( string path, Vertex[] vertices, Material material ) : this( path, material, false )
	{
		SetupMesh( vertices );
		CreateUniformBuffer();
		CreateResources();
	}

	private void SetupMesh( Vertex[] vertices )
	{
		VertexBuffer = new();

		int strideInBytes = Marshal.SizeOf( typeof( Vertex ) );
		int sizeInBytes = strideInBytes * vertices.Length;

		VertexBuffer.CreateVertexBuffer( sizeInBytes, strideInBytes );

		unsafe
		{
			fixed ( void* data = vertices )
			{
				VertexBuffer.SetData( (IntPtr)data );
			}
		}
	}

	private void SetupMesh( Vertex[] vertices, uint[] indices )
	{
		SetupMesh( vertices );

		IndexBuffer = new();
		indexCount = indices.Length;

		int sizeInBytes = Marshal.SizeOf( typeof( uint ) ) * indices.Length;
		IndexBuffer.CreateIndexBuffer( sizeInBytes );

		unsafe
		{
			fixed ( void* data = indices )
			{
				IndexBuffer.SetData( (IntPtr)data );
			}
		}
	}

	private void CreateResources()
	{
	}

	private void CreateUniformBuffer()
	{
	}

	public void Render()
	{
		if ( !IsIndexed )
		{
			Log.Error( "fuck off" );
			throw new Exception( "no" );
		}

		Glue.Renderer.DrawModel(
			Material.Shader.NativePtr,
			indexCount,
			VertexBuffer.NativePtr,
			IndexBuffer.NativePtr );
	}
}
