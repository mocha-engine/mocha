namespace Mocha.Renderer;

public class PipelineFactory
{
	private VertexElementDescription[] vertexElementDescriptions;
	private FaceCullMode faceCullMode = FaceCullMode.Back;
	private Shader shader;
	private Framebuffer framebuffer;

	public PipelineFactory() { }

	public PipelineFactory WithVertexElementDescriptions( VertexElementDescription[] vertexElementDescriptions )
	{
		this.vertexElementDescriptions = vertexElementDescriptions;
		return this;
	}


	public PipelineFactory WithFaceCullMode( FaceCullMode faceCullMode )
	{
		this.faceCullMode = faceCullMode;

		return this;
	}

	public PipelineFactory WithShader( Shader shader )
	{
		this.shader = shader;

		return this;
	}

	public PipelineFactory WithFramebuffer( Framebuffer framebuffer )
	{
		this.framebuffer = framebuffer;

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
		return new RenderPipeline( pipeline, objectResourceLayout, lightingResourceLayout );
	}
}
