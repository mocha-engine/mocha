using System.Runtime.InteropServices;
using Veldrid;

namespace Mocha;

public class Model
{
	private DeviceBuffer uniformBuffer;
	private Pipeline pipeline;
	private ResourceSet resourceSet;

	public DeviceBuffer TBNBuffer { get; private set; }
	public DeviceBuffer VertexBuffer { get; private set; }
	public DeviceBuffer IndexBuffer { get; private set; }

	public Material Material { get; private set; }

	public bool IsIndexed { get; private set; }

	private uint indexCount;
	private uint vertexCount;

	public Model( Vertex[] vertices, uint[] indices, Material material )
	{
		Material = material;
		IsIndexed = true;

		SetupMesh( vertices, indices );
		SetupResources( material );

		material.Shader.OnRecompile += () => SetupResources( material );
	}

	public Model( Vertex[] vertices, Material material )
	{
		Material = material;
		IsIndexed = false;

		SetupMesh( vertices );
		SetupResources( material );

		material.Shader.OnRecompile += () => SetupResources( material );
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

	private void SetupResources( Material material )
	{
		var vertexLayout = new VertexLayoutDescription( Vertex.VertexElementDescriptions );

		var rsrcLayoutDesc = new ResourceLayoutDescription()
		{
			Elements = new[]
			{
				new ResourceLayoutElementDescription()
				{
					Kind = ResourceKind.TextureReadOnly,
					Name = "g_tDiffuse",
					Options = ResourceLayoutElementOptions.None,
					Stages = ShaderStages.Fragment
				},
				new ResourceLayoutElementDescription()
				{
					Kind = ResourceKind.TextureReadOnly,
					Name = "g_tAlpha",
					Options = ResourceLayoutElementOptions.None,
					Stages = ShaderStages.Fragment
				},
				new ResourceLayoutElementDescription()
				{
					Kind = ResourceKind.TextureReadOnly,
					Name = "g_tNormal",
					Options = ResourceLayoutElementOptions.None,
					Stages = ShaderStages.Fragment
				},
				new ResourceLayoutElementDescription()
				{
					Kind = ResourceKind.TextureReadOnly,
					Name = "g_tORM",
					Options = ResourceLayoutElementOptions.None,
					Stages = ShaderStages.Fragment
				},
				new ResourceLayoutElementDescription()
				{
					Kind = ResourceKind.Sampler,
					Name = "g_sSampler",
					Options = ResourceLayoutElementOptions.None,
					Stages = ShaderStages.Fragment
				},
				new ResourceLayoutElementDescription()
				{
					Kind = ResourceKind.UniformBuffer,
					Name = "g_oUbo",
					Options = ResourceLayoutElementOptions.None,
					Stages = ShaderStages.Vertex | ShaderStages.Fragment
				}
			}
		};

		var rsrcLayout = Device.ResourceFactory.CreateResourceLayout( rsrcLayoutDesc );

		var pipelineDescription = new GraphicsPipelineDescription()
		{
			BlendState = BlendStateDescription.SingleAlphaBlend,

			DepthStencilState = new DepthStencilStateDescription(
				true,
				true,
				ComparisonKind.LessEqual ),

			RasterizerState = new RasterizerStateDescription(
				FaceCullMode.Back,
				PolygonFillMode.Solid,
				FrontFace.Clockwise,
				true,
				false ),

			PrimitiveTopology = PrimitiveTopology.TriangleList,
			ResourceLayouts = new[] { rsrcLayout },
			ShaderSet = new ShaderSetDescription( new[] { vertexLayout }, material.Shader.ShaderProgram ),
			Outputs = Device.SwapchainFramebuffer.OutputDescription
		};

		pipeline = Device.ResourceFactory.CreateGraphicsPipeline( pipelineDescription );

		uint uboSizeInBytes = 4 * (uint)Marshal.SizeOf( Material.UniformBufferType );
		uniformBuffer = Device.ResourceFactory.CreateBuffer(
			new BufferDescription( uboSizeInBytes,
				BufferUsage.UniformBuffer | BufferUsage.Dynamic ) );

		var resourceSetDescription = new ResourceSetDescription(
			rsrcLayout,
			material.DiffuseTexture?.VeldridTexture ?? TextureBuilder.One.VeldridTexture,
			material.AlphaTexture?.VeldridTexture ?? TextureBuilder.One.VeldridTexture,
			material.NormalTexture?.VeldridTexture ?? TextureBuilder.Zero.VeldridTexture,
			material.ORMTexture?.VeldridTexture ?? TextureBuilder.Zero.VeldridTexture,
			Device.Aniso4xSampler,
			uniformBuffer );

		resourceSet = Device.ResourceFactory.CreateResourceSet( resourceSetDescription );
	}

	internal void Draw<T>( T uniformBufferContents, CommandList commandList ) where T : struct
	{
		if ( uniformBufferContents.GetType() != Material.UniformBufferType )
		{
			throw new Exception( $"Tried to set unmatching uniform buffer object" +
				$" of type {uniformBufferContents.GetType()}, expected {Material.UniformBufferType}" );
		}

		commandList.SetVertexBuffer( 0, VertexBuffer );
		commandList.SetPipeline( pipeline );

		Device.UpdateBuffer( uniformBuffer, 0, new[] { uniformBufferContents } );
		commandList.SetGraphicsResourceSet( 0, resourceSet );

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
