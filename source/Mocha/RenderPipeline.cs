namespace Mocha.Renderer;

public struct RenderPipeline
{
	public static PipelineFactory Factory => new();

	public ResourceLayout[] ResourceLayouts;
	public Pipeline Pipeline;

	public RenderPipeline( Pipeline pipeline, params ResourceLayout[] resourceSets )
	{
		this.ResourceLayouts = resourceSets;
		this.Pipeline = pipeline;
	}

	public void Delete()
	{
		Pipeline?.Dispose();
		ResourceLayouts?.ToList().ForEach( x => x.Dispose() );
	}
}
