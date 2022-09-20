using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Veldrid;

namespace Mocha.Renderer.UI;

[Icon( FontAwesome.Square ), Title( "UI" )]
public class PanelRenderer : Asset
{
	private bool isDirty = false;

	private DeviceBuffer uniformBuffer;

	public DeviceBuffer VertexBuffer { get; private set; }
	public DeviceBuffer IndexBuffer { get; private set; }
	public Material Material { get; set; }

	private ResourceSet objectResourceSet;
	private uint indexCount;
	private uint vertexCount;

	private UIVertex[] RectVertices => new UIVertex[] {
		new UIVertex { Position = new ( 0, 0, 0 ), TexCoords = new( 0, 0 ) },
		new UIVertex { Position = new ( 0, 1, 0 ), TexCoords = new( 0, 1 ) },
		new UIVertex { Position = new ( 1, 0, 0 ), TexCoords = new( 1, 0 ) },
		new UIVertex { Position = new ( 1, 1, 0 ), TexCoords = new( 1, 1 ) },
	};

	private uint[] RectIndices => new uint[] {
		2, 1, 0,
		1, 2, 3
	};

	private int RectCount = 0;
	private List<UIVertex> Vertices = new();

	[StructLayout( LayoutKind.Sequential )]
	public struct UIUniformBuffer
	{
		/*
		 * These fields are padded so that they're
		 * aligned (as blocks) to multiples of 16.
		 */

		public Vector4 vSdfRange;
	}

	[StructLayout( LayoutKind.Sequential )]
	public struct UIVertex
	{
		public Vector3 Position { get; set; }
		public Vector2 TexCoords { get; set; }
		public Vector4 Color { get; set; }
		public float ScreenPxRange { get; set; }

		public static VertexElementDescription[] VertexElementDescriptions = new[]
		{
			new VertexElementDescription( "position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3 ),
			new VertexElementDescription( "texCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2 ),
			new VertexElementDescription( "color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4 ),
			new VertexElementDescription( "screenPxRange", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1 ),
		};
	}

	public PanelRenderer( Texture atlasTexture )
	{
		Path = "internal:ui_panel";

		var pipeline = RenderPipeline.Factory
			.WithVertexElementDescriptions( UIVertex.VertexElementDescriptions )
			.AddObjectResource( "g_tAtlas", ResourceKind.TextureReadOnly, ShaderStages.Fragment )
			.AddObjectResource( "g_sSampler", ResourceKind.Sampler, ShaderStages.Fragment )
			.AddObjectResource( "g_oUbo", ResourceKind.UniformBuffer, ShaderStages.Fragment | ShaderStages.Vertex );

		var shader = ShaderBuilder.Default.FromPath( "core/shaders/ui/ui.mshdr" )
			.WithFramebuffer( Device.SwapchainFramebuffer )
			.WithCustomPipeline( pipeline )
			.Build();

		Material = new Material()
		{
			UniformBufferType = typeof( UIUniformBuffer ),
			DiffuseTexture = atlasTexture,
			Shader = shader
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

	private void UpdateVertexBuffer( UIVertex[] vertices )
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
			Device.Aniso4xSampler,
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

	private void InternalAddRectangle( Common.Rectangle rect, Common.Rectangle ndcTexRect, Vector4 colorA, Vector4 colorB, Vector4 colorC, Vector4 colorD )
	{
		var ndcRect = rect / (Vector2)Screen.Size;
		var vertices = RectVertices.Select( ( x, i ) =>
		{
			var position = x.Position;
			position.X = (x.Position.X * ndcRect.Size.X) + ndcRect.Position.X;
			position.Y = (x.Position.Y * ndcRect.Size.Y) + ndcRect.Position.Y;

			var texCoords = x.TexCoords;
			texCoords.X = (x.TexCoords.X * ndcTexRect.Size.X) + ndcTexRect.Position.X;
			texCoords.Y = (x.TexCoords.Y * ndcTexRect.Size.Y) + ndcTexRect.Position.Y;

			var screenPxRange = (rect.Size.Length / 28f);
			screenPxRange *= 3.0f;

			var tx = x;
			position *= 2.0f;
			position.X -= 1.0f;
			position.Y = 1.0f - position.Y;

			tx.Position = position;
			tx.TexCoords = texCoords;
			tx.ScreenPxRange = texCoords.Length > 0 ? screenPxRange : 0f;
			tx.Color = i switch
			{
				0 => colorA,
				1 => colorB,
				2 => colorC,
				3 => colorD,
				_ => Vector4.Zero,
			};

			return tx;
		} ).ToArray();

		Vertices.AddRange( vertices );
		RectCount++;
		isDirty = true;
	}

	public void AddRectangle( Common.Rectangle rect, Vector4 colorA, Vector4 colorB, Vector4 colorC, Vector4 colorD )
	{
		InternalAddRectangle( rect, new Common.Rectangle( 0, 0, 0, 0 ), colorA, colorB, colorC, colorD );
	}

	public void AddRectangle( Common.Rectangle rect, Common.Rectangle ndcTexRect, Vector4 color )
	{
		InternalAddRectangle( rect, ndcTexRect, color, color, color, color );
	}

	public void AddRectangle( Common.Rectangle rect, Vector4 color )
	{
		InternalAddRectangle( rect, new Common.Rectangle( 0, 0, 0, 0 ), color, color, color, color );
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
		{
			UpdateBuffers();
		}

		RenderPipeline renderPipeline = Material.Shader.Pipeline;

		var uniformBufferContents = new UIUniformBuffer()
		{
			vSdfRange = new Vector4( 0.1f, 0.0f, 0.1f, 0.1f )
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
