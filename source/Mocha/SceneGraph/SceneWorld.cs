namespace Mocha.Renderer;

public class SceneWorld
{
	public static SceneWorld Current { get; private set; }

	public SceneCamera Camera => SceneObject.All.OfType<SceneCamera>().FirstOrDefault();
	public SceneLight Sun => SceneObject.All.OfType<SceneLight>().FirstOrDefault();

	public SceneWorld()
	{
		Current = this;
	}

	public void Render( Matrix4x4 viewProjMatrix, RenderPass renderPass, CommandList commandList )
	{
		SceneObject.All.ForEach( sceneObject => sceneObject.Render( viewProjMatrix, renderPass, commandList ) );
	}
}
