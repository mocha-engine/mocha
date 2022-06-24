using System.Runtime.Versioning;

namespace Mocha.Renderer;

public static class PipelineManager
{
	struct CachedPipeline
	{
		public Material Material { get; set; }
		public Framebuffer Framebuffer { get; set; }
		public VertexElementDescription[] VertexElementDescriptions { get; set; }
		public DeviceBuffer UniformBuffer { get; set; }
		public Pipeline Pipeline { get; set; }
		public ResourceSet ResourceSet { get; set; }

		public bool IsMatch( Material Material, Framebuffer Framebuffer,
			VertexElementDescription[] VertexElementDescriptions,
			DeviceBuffer UniformBuffer )
		{
			return this.Material.GetHashCode() == Material.GetHashCode()
				&& this.Framebuffer.GetHashCode() == Framebuffer.GetHashCode()
				&& this.VertexElementDescriptions.GetHashCode() == VertexElementDescriptions.GetHashCode()
				&& this.UniformBuffer.GetHashCode() == UniformBuffer.GetHashCode();
		}
	}

	private static List<CachedPipeline?> PipelineCache { get; set; } = new();

	private static ResourceLayout CreateResourceLayout()
	{
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
		return rsrcLayout;
	}

	private static Pipeline CreatePipeline( Shader shader, Framebuffer framebuffer,
		VertexLayoutDescription vertexLayoutDescription, ResourceLayout resourceLayout )
	{
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
			ResourceLayouts = new[] { resourceLayout },
			ShaderSet = new ShaderSetDescription( new[] { vertexLayoutDescription }, shader.ShaderProgram ),
			Outputs = framebuffer.OutputDescription
		};

		var pipeline = Device.ResourceFactory.CreateGraphicsPipeline( pipelineDescription );
		return pipeline;
	}

	private static ResourceSet CreateResourceSet( ResourceLayout resourceLayout, Material material, DeviceBuffer uniformBuffer )
	{
		var resourceSetDescription = new ResourceSetDescription(
			resourceLayout,
			material.DiffuseTexture?.VeldridTexture ?? TextureBuilder.One.VeldridTexture,
			material.AlphaTexture?.VeldridTexture ?? TextureBuilder.One.VeldridTexture,
			material.NormalTexture?.VeldridTexture ?? TextureBuilder.Zero.VeldridTexture,
			material.ORMTexture?.VeldridTexture ?? TextureBuilder.Zero.VeldridTexture,
			Device.Aniso4xSampler,
			uniformBuffer );

		var resourceSet = Device.ResourceFactory.CreateResourceSet( resourceSetDescription );
		return resourceSet;
	}

	public static (Pipeline, ResourceSet) GetPipelineAndResourceSetFor<T>(
		Material material, Framebuffer framebuffer, VertexElementDescription[] vertexElementDescriptions, DeviceBuffer uniformBuffer ) where T : struct
	{
		var cachedPipeline = PipelineCache
			.FirstOrDefault( x => x?.IsMatch( material, framebuffer, vertexElementDescriptions, uniformBuffer ) ?? false );

		if ( cachedPipeline.HasValue )
			return (cachedPipeline.Value.Pipeline, cachedPipeline.Value.ResourceSet);

		Log.Info( $"Creating a new pipeline because there wasn't one cached that matched" );
		using var _ = new Stopwatch( "Pipeline creation" );

		var vertexLayoutDescription = new VertexLayoutDescription( vertexElementDescriptions );
		var resourceLayout = CreateResourceLayout();
		var pipeline = CreatePipeline( material.Shader, framebuffer, vertexLayoutDescription, resourceLayout );
		var resourceSet = CreateResourceSet( resourceLayout, material, uniformBuffer );

		PipelineCache.Add( new CachedPipeline()
		{
			Pipeline = pipeline,
			ResourceSet = resourceSet,
			Framebuffer = framebuffer,
			Material = material,
			UniformBuffer = uniformBuffer,
			VertexElementDescriptions = vertexElementDescriptions
		} );

		return (pipeline, resourceSet);
	}
}
