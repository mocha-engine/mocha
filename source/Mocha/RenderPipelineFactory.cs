namespace Mocha.Renderer;

public class RenderPipelineFactory
{
	// TODO: Sensible defaults for all of these
	private VertexElementDescription[] vertexElementDescriptions;
	private Material material;
	private FaceCullMode faceCullMode = FaceCullMode.Back;
	private Shader shader;
	private Framebuffer framebuffer;
	private DeviceBuffer uniformBuffer;

	public RenderPipelineFactory() { }

	public RenderPipelineFactory WithVertexElementDescriptions( VertexElementDescription[] vertexElementDescriptions )
	{
		this.vertexElementDescriptions = vertexElementDescriptions;
		return this;
	}

	public RenderPipelineFactory WithMaterial( Material material )
	{
		this.shader = material.Shader;
		this.material = material;
		return this;
	}

	public RenderPipelineFactory WithFaceCullMode( FaceCullMode faceCullMode )
	{
		this.faceCullMode = faceCullMode;

		return this;
	}

	public RenderPipelineFactory WithFramebuffer( Framebuffer framebuffer )
	{
		this.framebuffer = framebuffer;

		return this;
	}

	public RenderPipelineFactory WithUniformBuffer( DeviceBuffer uniformBuffer )
	{
		this.uniformBuffer = uniformBuffer;

		return this;
	}

	public RenderPipeline Build()
	{
		var vertexLayoutDescription = new VertexLayoutDescription( vertexElementDescriptions );
		var objectResourceLayoutDescription = new ResourceLayoutDescription()
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
		var objectResourceLayout = Device.ResourceFactory.CreateResourceLayout( objectResourceLayoutDescription );

		var lightingResourceLayoutDescription = new ResourceLayoutDescription()
		{
			Elements = new[]
			{
				new ResourceLayoutElementDescription()
				{
					Kind = ResourceKind.TextureReadOnly,
					Name = "g_tShadowMap",
					Options = ResourceLayoutElementOptions.None,
					Stages = ShaderStages.Fragment
				},
				new ResourceLayoutElementDescription()
				{
					Kind = ResourceKind.Sampler,
					Name = "g_sSampler",
					Options = ResourceLayoutElementOptions.None,
					Stages = ShaderStages.Fragment
				}
			}
		};

		var lightingResourceLayout = Device.ResourceFactory.CreateResourceLayout( lightingResourceLayoutDescription );

		var pipelineDescription = new GraphicsPipelineDescription()
		{
			BlendState = BlendStateDescription.SingleAlphaBlend,

			DepthStencilState = new DepthStencilStateDescription(
				true,
				true,
				ComparisonKind.LessEqual ),

			RasterizerState = new RasterizerStateDescription(
				faceCullMode,
				PolygonFillMode.Solid,
				FrontFace.Clockwise,
				true,
				false ),

			PrimitiveTopology = PrimitiveTopology.TriangleList,
			ResourceLayouts = new[] { objectResourceLayout, lightingResourceLayout },
			ShaderSet = new ShaderSetDescription( new[] { vertexLayoutDescription }, shader.ShaderProgram ),
			Outputs = framebuffer.OutputDescription
		};

		var pipeline = Device.ResourceFactory.CreateGraphicsPipeline( pipelineDescription );

		var objectResourceSetDescription = new ResourceSetDescription(
			objectResourceLayout,
			material.DiffuseTexture?.VeldridTexture ?? TextureBuilder.One.VeldridTexture,
			material.AlphaTexture?.VeldridTexture ?? TextureBuilder.One.VeldridTexture,
			material.NormalTexture?.VeldridTexture ?? TextureBuilder.Zero.VeldridTexture,
			material.ORMTexture?.VeldridTexture ?? TextureBuilder.Zero.VeldridTexture,
			Device.Aniso4xSampler,
			uniformBuffer );

		var objectResourceSet = Device.ResourceFactory.CreateResourceSet( objectResourceSetDescription );

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
			lightingResourceLayout,
			SceneWorld.Current.Sun.DepthTexture.VeldridTexture,
			shadowSampler );

		var lightingResourceSet = Device.ResourceFactory.CreateResourceSet( lightingResourceSetDescription );

		var renderPipeline = new RenderPipeline( pipeline, objectResourceSet, lightingResourceSet );
		return renderPipeline;
	}
}
