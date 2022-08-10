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
}
