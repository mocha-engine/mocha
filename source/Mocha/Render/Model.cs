using System.Runtime.InteropServices;
using Veldrid;

namespace Mocha.Renderer;

public class Model
{
	private DeviceBuffer uniformBuffer;

	public DeviceBuffer TBNBuffer { get; private set; }
	public DeviceBuffer VertexBuffer { get; private set; }
	public DeviceBuffer IndexBuffer { get; private set; }

	public Material Material { get; private set; }

	private RenderPipeline mainRenderPipeline;
	private RenderPipeline shadowRenderPipeline;

	public bool IsIndexed { get; private set; }

	private uint indexCount;
	private uint vertexCount;

	public Model( Vertex[] vertices, uint[] indices, Material material )
	{
		Material = material;
		IsIndexed = true;

		SetupMesh( vertices, indices );
		CreateUniformBuffer();
		CreateResources();

		Material.Shader.OnRecompile += CreateResources;
	}

	public Model( Vertex[] vertices, Material material )
	{
		Material = material;
		IsIndexed = false;

		SetupMesh( vertices );
		CreateUniformBuffer();
		CreateResources();

		Material.Shader.OnRecompile += CreateResources;
	}

	private void SetupMesh( Vertex[] vertices )
	{
		var factory = Device.ResourceFactory;
		var vertexStructSize = (uint)Marshal.SizeOf( typeof( Vertex ) );
		vertexCount = (uint)vertices.Length;

		VertexBuffer = factory.CreateBuffer(
			new Veldrid.BufferDescription( vertexCount * vertexStructSize, Veldrid.BufferUsage.VertexBuffer )
		);

		Device.UpdateBuffer( VertexBuffer, 0, vertices );
	}

	private void SetupMesh( Vertex[] vertices, uint[] indices )
	{
		SetupMesh( vertices );

		var factory = Device.ResourceFactory;
		indexCount = (uint)indices.Length;

		IndexBuffer = factory.CreateBuffer(
			new Veldrid.BufferDescription( indexCount * sizeof( uint ), Veldrid.BufferUsage.IndexBuffer )
		);

		Device.UpdateBuffer( IndexBuffer, 0, indices );
	}

	private void CreateResources()
	{
		mainRenderPipeline.Delete();
		shadowRenderPipeline.Delete();

		mainRenderPipeline = RenderPipeline.Factory
			.WithVertexElementDescriptions( Vertex.VertexElementDescriptions )
			.WithMaterial( Material )
			.WithUniformBuffer( uniformBuffer )
			.WithFramebuffer( Device.SwapchainFramebuffer )
			.Build();

		shadowRenderPipeline = RenderPipeline.Factory
			.WithVertexElementDescriptions( Vertex.VertexElementDescriptions )
			.WithMaterial( Material )
			.WithUniformBuffer( uniformBuffer )
			.WithFramebuffer( SceneWorld.Current.Sun.ShadowBuffer )
			.WithFaceCullMode( FaceCullMode.None )
			.Build();
	}

	private void CreateUniformBuffer()
	{
		uint uboSizeInBytes = 4 * (uint)Marshal.SizeOf( Material.UniformBufferType );
		uniformBuffer = Device.ResourceFactory.CreateBuffer(
			new BufferDescription( uboSizeInBytes,
				BufferUsage.UniformBuffer | BufferUsage.Dynamic ) );
	}

	public void Draw<T>( RenderPass renderPass, T uniformBufferContents, CommandList commandList ) where T : struct
	{
		if ( uniformBufferContents.GetType() != Material.UniformBufferType )
		{
			throw new Exception( $"Tried to set unmatching uniform buffer object" +
				$" of type {uniformBufferContents.GetType()}, expected {Material.UniformBufferType}" );
		}

		RenderPipeline renderPipeline = renderPass switch
		{
			RenderPass.Main => mainRenderPipeline,
			RenderPass.ShadowMap => shadowRenderPipeline,

			_ => throw new NotImplementedException(),
		};

		commandList.SetVertexBuffer( 0, VertexBuffer );
		commandList.UpdateBuffer( uniformBuffer, 0, new[] { uniformBufferContents } );
		commandList.SetPipeline( renderPipeline.Pipeline );

		for ( uint i = 0; i < renderPipeline.ResourceSets.Length; i++ )
		{
			commandList.SetGraphicsResourceSet( i, renderPipeline.ResourceSets[i] );
		}

		if ( IsIndexed )
		{
			commandList.SetIndexBuffer( IndexBuffer, IndexFormat.UInt32 );

			commandList.DrawIndexed(
				indexCount: indexCount,
				instanceCount: 1,
				indexStart: 0,
				vertexOffset: 0,
				instanceStart: 0
			);
		}
		else
		{
			commandList.Draw( vertexCount );
		}
	}
}
