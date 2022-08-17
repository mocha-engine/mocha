using System;
using System.Runtime.InteropServices;

namespace Mocha.Renderer;

[Icon( FontAwesome.Cube ), Title( "Model" )]
public class Model : Asset
{
	private Glue.CDeviceBuffer VertexBuffer { get; set; }
	private Glue.CDeviceBuffer IndexBuffer { get; set; }

	public Material Material { get; set; }
	public bool IsIndexed { get; private set; }

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

		int sizeInBytes = Marshal.SizeOf( typeof( uint ) ) * indices.Length;
		IndexBuffer.CreateIndexBuffer( sizeInBytes );

		// Debug: show indices in console
		for ( int i = 0; i < indices.Length; i++ )
		{
			uint index = indices[i];
			Log.Info( $"[C#] Index {i}: {index}" );
		}

		unsafe
		{
			fixed ( void* data = indices )
			{
				var ptr = (IntPtr)data;
				IndexBuffer.SetData( ptr );
				Log.Trace( $"[C#] Pointer: 0x{ptr.ToInt64():x}" );
			}
		}

		// pinnedIndices.Free();
	}

	private void CreateResources()
	{
	}

	private void CreateUniformBuffer()
	{
	}
}
