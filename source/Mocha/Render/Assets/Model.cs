using System.Runtime.InteropServices;

namespace Mocha.Renderer;

[Icon( FontAwesome.Cube ), Title( "Model" )]
public class Model : Asset
{
	private DeviceBuffer uniformBuffer;

	public DeviceBuffer TBNBuffer { get; private set; }
	public DeviceBuffer VertexBuffer { get; private set; }
	public DeviceBuffer IndexBuffer { get; private set; }

	public Material Material { get; set; }
	private Material DepthOnlyMaterial { get; set; }

	private ResourceSet objectResourceSet;
	private ResourceSet lightingResourceSet;

	public bool IsIndexed { get; private set; }

	private uint indexCount;
	private uint vertexCount;

	public Model( string path, Material material, bool isIndexed )
	{
		DepthOnlyMaterial = new Material()
		{
			Shader = ShaderBuilder.Default.FromPath( "shaders/depthonly.mshdr" )
										  .WithFaceCullMode( FaceCullMode.None )
										  .WithFramebuffer( SceneWorld.Current.Sun.ShadowBuffer )
										  .Build(),
			UniformBufferType = typeof( GenericModelUniformBuffer )
		};

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

		var shadowSamplerDescription = new SamplerDescription(
			SamplerAddressMode.Border,
			SamplerAddressMode.Border,
			SamplerAddressMode.Border,

			SamplerFilter.Anisotropic,
			null,
			16,
			0,
			uint.MaxValue,
			0,
			SamplerBorderColor.OpaqueBlack
		);

		var shadowSampler = Device.ResourceFactory.CreateSampler( shadowSamplerDescription );

		var lightingResourceSetDescription = new ResourceSetDescription(
			Material.Shader.Pipeline.ResourceLayouts[1],
			SceneWorld.Current.Sun.DepthTexture.VeldridTexture,
			shadowSampler );

		lightingResourceSet = Device.ResourceFactory.CreateResourceSet( lightingResourceSetDescription );
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
			RenderPass.Main => Material.Shader.Pipeline,
			RenderPass.ShadowMap => DepthOnlyMaterial.Shader.Pipeline,
			RenderPass.Combine => Material.Shader.Pipeline,

			_ => throw new NotImplementedException(),
		};

		commandList.SetVertexBuffer( 0, VertexBuffer );
		commandList.UpdateBuffer( uniformBuffer, 0, new[] { uniformBufferContents } );
		commandList.SetPipeline( renderPipeline.Pipeline );

		commandList.SetGraphicsResourceSet( 0, objectResourceSet );
		commandList.SetGraphicsResourceSet( 1, lightingResourceSet );

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
