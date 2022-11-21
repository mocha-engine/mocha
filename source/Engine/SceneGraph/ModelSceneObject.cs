using Mocha.Common.World;

namespace Mocha.Renderer;

public class ModelSceneObject : SceneObject
{
	public List<Model> models;

	public ModelSceneObject( IEntity entity )
	{
	}

	public void SetModels( List<Model> models )
	{
		this.models = models;
	}

	public override void Render()
	{
		base.Render();

		models.ForEach( x => x.Render() );
	}
}
