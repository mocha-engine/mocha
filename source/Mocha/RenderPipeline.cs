namespace Mocha.Renderer;

public struct RenderPipeline
{
	public static RenderPipelineFactory Factory => new();

	public ResourceSet[] ResourceSets;
	public Pipeline Pipeline;

	public RenderPipeline( Pipeline pipeline, params ResourceSet[] resourceSets )
	{
		this.ResourceSets = resourceSets;
		this.Pipeline = pipeline;
	}

	public void Delete()
	{
		Pipeline?.Dispose();
		ResourceSets?.ToList().ForEach( x => x.Dispose() );
	}
}
