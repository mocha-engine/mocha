using System.Runtime.InteropServices;

namespace Mocha.Renderer.UI;

[Icon( FontAwesome.Square ), Title( "UI" )]
public class PanelRenderer : Asset
{
	private DeviceBuffer uniformBuffer;

	public DeviceBuffer VertexBuffer { get; private set; }
	public DeviceBuffer IndexBuffer { get; private set; }

	public Material Material { get; set; }

	private ResourceSet objectResourceSet;

	public bool IsIndexed { get; private set; }

	private uint indexCount;
	private uint vertexCount;

	public PanelRenderer()
	{
		Path = "internal:ui_panel";
		IsIndexed = true;

		Material = new Material()
		{
			UniformBufferType = typeof( EmptyUniformBuffer ),
			Shader = ShaderBuilder.Default.FromPath( "core/shaders/ui/ui.mshdr" ).WithFramebuffer( Device.SwapchainFramebuffer ).Build()
		};

		var vertices = new Vertex[] {
			new Vertex { Position = new ( 0, 0, 0 ) },
			new Vertex { Position = new ( 0, 1, 0 ) },
			new Vertex { Position = new ( 1, 0, 0 ) },
			new Vertex { Position = new ( 1, 1, 0 ) },
		};

		var indices = new uint[]
		{
			0, 1, 2,
			3, 2, 1
		};

		SetupMesh( vertices, indices );
		CreateUniformBuffer();
		CreateResources();

		All.Add( this );
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
		var objectResourceSetDescription = new ResourceSetDescription(
			Material.Shader.Pipeline.ResourceLayouts[0],
			Material.DiffuseTexture?.VeldridTexture ?? TextureBuilder.MissingTexture.VeldridTexture,
			Material.AlphaTexture?.VeldridTexture ?? TextureBuilder.One.VeldridTexture,
			Material.NormalTexture?.VeldridTexture ?? TextureBuilder.Zero.VeldridTexture,
			Material.ORMTexture?.VeldridTexture ?? TextureBuilder.Zero.VeldridTexture,
			Device.PointSampler,
			uniformBuffer );

		objectResourceSet = Device.ResourceFactory.CreateResourceSet( objectResourceSetDescription );
	}

	private void CreateUniformBuffer()
	{
		uint uboSizeInBytes = 4 * (uint)Marshal.SizeOf( Material.UniformBufferType );
		uniformBuffer = Device.ResourceFactory.CreateBuffer(
			new BufferDescription( uboSizeInBytes,
				BufferUsage.UniformBuffer | BufferUsage.Dynamic ) );
	}

	public void Draw<T>( T uniformBufferContents, CommandList commandList ) where T : struct
	{
		if ( uniformBufferContents.GetType() != Material.UniformBufferType )
		{
			throw new Exception( $"Tried to set unmatching uniform buffer object" +
				$" of type {uniformBufferContents.GetType()}, expected {Material.UniformBufferType}" );
		}

		RenderPipeline renderPipeline = Material.Shader.Pipeline;

		commandList.SetVertexBuffer( 0, VertexBuffer );
		commandList.UpdateBuffer( uniformBuffer, 0, new[] { uniformBufferContents } );
		commandList.SetPipeline( renderPipeline.Pipeline );

		commandList.SetGraphicsResourceSet( 0, objectResourceSet );

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
