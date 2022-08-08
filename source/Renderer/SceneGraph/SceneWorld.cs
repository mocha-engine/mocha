namespace Mocha.Renderer;

public class SceneWorld
{
	public static SceneWorld Current { get; private set; }

	public SceneCamera Camera { get; set; } = new SceneCamera();
	public SceneLight Sun => SceneObject.All.OfType<SceneLight>().FirstOrDefault();

	public SceneWorld()
	{
		Current = this;
	}

	public void Render( Matrix4x4 viewProjMatrix, RenderPass renderPass, CommandList commandList )
	{
		SceneObject.All.ForEach( sceneObject =>
		{
			if ( sceneObject.IsVisible )
				sceneObject.Render( viewProjMatrix, renderPass, commandList );
		} );
	}
}
