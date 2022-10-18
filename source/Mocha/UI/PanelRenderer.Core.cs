using System.Runtime.InteropServices;

namespace Mocha.Renderer.UI;
partial class PanelRenderer
{
	private ResourceSet objectResourceSet;
	private uint indexCount;
	private bool isDirty = false;

	private DeviceBuffer uniformBuffer;
	private DeviceBuffer vertexBuffer;
	private DeviceBuffer indexBuffer;
	private Material material;

	private void UpdateIndexBuffer( uint[] indices )
	{
		indexCount = (uint)indices.Length;
		var targetSize = indexCount * sizeof( uint );

		if ( indexBuffer == null || indexBuffer.SizeInBytes != targetSize )
		{
			indexBuffer?.Dispose();

			indexBuffer = Device.ResourceFactory.CreateBuffer(
				new Veldrid.BufferDescription( targetSize, Veldrid.BufferUsage.IndexBuffer )
			);
		}

		Device.UpdateBuffer( indexBuffer, 0, indices );
	}

	private void UpdateVertexBuffer( UIVertex[] vertices )
	{
		var vertexStructSize = (uint)Marshal.SizeOf( typeof( Vertex ) );
		var targetSize = (uint)vertices.Length * vertexStructSize;

		if ( vertexBuffer == null || vertexBuffer.SizeInBytes != targetSize )
		{
			vertexBuffer?.Dispose();

			vertexBuffer = Device.ResourceFactory.CreateBuffer(
				new Veldrid.BufferDescription( targetSize, Veldrid.BufferUsage.VertexBuffer )
			);
		}

		Device.UpdateBuffer( vertexBuffer, 0, vertices );
	}

	private void CreateResources()
	{
		Log.Info( $"Updating PanelRenderer object resource set" );

		material.DiffuseTexture = AtlasBuilder.Texture;

		var objectResourceSetDescription = new ResourceSetDescription(
			material.Shader.Pipeline.ResourceLayouts[0],
			material.DiffuseTexture?.VeldridTexture ?? TextureBuilder.MissingTexture.VeldridTexture,
			Device.Aniso4xSampler,
			uniformBuffer );

		objectResourceSet = Device.ResourceFactory.CreateResourceSet( objectResourceSetDescription );
	}

	private void CreateUniformBuffer()
	{
		uint uboSizeInBytes = 4 * (uint)Marshal.SizeOf( material.UniformBufferType );
		uniformBuffer = Device.ResourceFactory.CreateBuffer(
			new BufferDescription( uboSizeInBytes,
				BufferUsage.UniformBuffer | BufferUsage.Dynamic ) );
	}

	private void UpdateBuffers()
	{
		UpdateVertexBuffer( Vertices.ToArray() );
		var generatedIndices = new List<uint>();

		for ( int i = 0; i < RectCount; ++i )
		{
			generatedIndices.AddRange( RectIndices.Select( x => (uint)(x + i * 4) ).ToArray() );
		}

		UpdateIndexBuffer( generatedIndices.ToArray() );
		isDirty = false;
	}

	public void Draw( CommandList commandList )
	{
		if ( isDirty )
			UpdateBuffers();

		if ( vertexBuffer == null || indexBuffer == null )
			return;

		if ( objectResourceSet == null )
			return;

		RenderPipeline renderPipeline = material.Shader.Pipeline;

		var uniformBufferContents = new UIUniformBuffer()
		{
			vSdfRange = new Vector4( 0.1f, 0.0f, 0.1f, 0.1f )
		};

		commandList.SetVertexBuffer( 0, vertexBuffer );
		commandList.UpdateBuffer( uniformBuffer, 0, new[] { uniformBufferContents } );
		commandList.SetPipeline( renderPipeline.Pipeline );

		commandList.SetGraphicsResourceSet( 0, objectResourceSet );

		commandList.SetIndexBuffer( indexBuffer, IndexFormat.UInt32 );

		commandList.DrawIndexed(
			indexCount: indexCount,
			instanceCount: 1,
			indexStart: 0,
			vertexOffset: 0,
			instanceStart: 0
		);
	}
}
