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
	private uint indexCount;
	private uint vertexCount;

	private Vertex[] RectVertices => new Vertex[] {
		new Vertex { Position = new ( 0, 0, 0 ) },
		new Vertex { Position = new ( 0, 1, 0 ) },
		new Vertex { Position = new ( 1, 0, 0 ) },
		new Vertex { Position = new ( 1, 1, 0 ) },
	};

	private uint[] RectIndices => new uint[] {
		2, 1, 0,
		1, 2, 3
	};

	private int RectCount = 0;
	private List<Vertex> Vertices = new();

	[StructLayout( LayoutKind.Sequential )]
	public struct UIUniformBuffer
	{
		/*
		 * These fields are padded so that they're
		 * aligned (as blocks) to multiples of 16.
		 */

		public Vector3 vColor;
		public float flTime;
	}

	public PanelRenderer()
	{
		Path = "internal:ui_panel";

		Material = new Material()
		{
			UniformBufferType = typeof( UIUniformBuffer ),
			Shader = ShaderBuilder.Default.FromPath( "core/shaders/ui/ui.mshdr" ).WithFramebuffer( Device.SwapchainFramebuffer ).Build()
		};

		CreateUniformBuffer();
		CreateResources();

		All.Add( this );
		Material.Shader.OnRecompile += CreateResources;
	}

	private void UpdateIndexBuffer( uint[] indices )
	{
		indexCount = (uint)indices.Length;
		var targetSize = indexCount * sizeof( uint );

		if ( IndexBuffer == null || IndexBuffer.SizeInBytes != targetSize )
		{
			IndexBuffer?.Dispose();

			IndexBuffer = Device.ResourceFactory.CreateBuffer(
				new Veldrid.BufferDescription( targetSize, Veldrid.BufferUsage.IndexBuffer )
			);
		}

		Device.UpdateBuffer( IndexBuffer, 0, indices );
	}

	private void UpdateVertexBuffer( Vertex[] vertices )
	{
		var vertexStructSize = (uint)Marshal.SizeOf( typeof( Vertex ) );
		vertexCount = (uint)vertices.Length;
		var targetSize = vertexCount * vertexStructSize;

		if ( VertexBuffer == null || VertexBuffer.SizeInBytes != targetSize )
		{
			VertexBuffer?.Dispose();

			VertexBuffer = Device.ResourceFactory.CreateBuffer(
				new Veldrid.BufferDescription( targetSize, Veldrid.BufferUsage.VertexBuffer )
			);
		}

		Device.UpdateBuffer( VertexBuffer, 0, vertices );
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

	public void NewFrame()
	{
		Vertices.Clear();
		RectCount = 0;
	}

	public void AddRectangle( Common.Rectangle rect, Vector3 color )
	{
		var ndcRect = rect / (Vector2)Screen.Size;
		var vertices = RectVertices.Select( x =>
		{
			var position = x.Position;
			position.X = (x.Position.X * ndcRect.Size.X) + ndcRect.Position.X;
			position.Y = (x.Position.Y * ndcRect.Size.Y) + ndcRect.Position.Y;

			var tx = x;
			position.X -= 1.0f;
			position.Y = 1.0f - position.Y;
			tx.Position = position;

			return tx;
		} ).ToArray();

		Vertices.AddRange( vertices );
		RectCount++;
	}

	public void Draw( CommandList commandList )
	{
		{
			UpdateVertexBuffer( Vertices.ToArray() );
			var generatedIndices = new List<uint>();

			for ( int i = 0; i < RectCount; ++i )
			{
				generatedIndices.AddRange( RectIndices.Select( x => (uint)(x + i * 4) ).ToArray() );
			}

			UpdateIndexBuffer( generatedIndices.ToArray() );
		}

		RenderPipeline renderPipeline = Material.Shader.Pipeline;

		var uniformBufferContents = new UIUniformBuffer()
		{
			vColor = new Vector3( 0.15f, 0f, 0.15f ),
			flTime = Time.Now
		};

		commandList.SetVertexBuffer( 0, VertexBuffer );
		commandList.UpdateBuffer( uniformBuffer, 0, new[] { uniformBufferContents } );
		commandList.SetPipeline( renderPipeline.Pipeline );

		commandList.SetGraphicsResourceSet( 0, objectResourceSet );

		commandList.SetIndexBuffer( IndexBuffer, IndexFormat.UInt32 );

		commandList.DrawIndexed(
			indexCount: indexCount,
			instanceCount: 1,
			indexStart: 0,
			vertexOffset: 0,
			instanceStart: 0
		);
	}
}
