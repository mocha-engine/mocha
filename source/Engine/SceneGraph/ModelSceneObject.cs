using Mocha.Common.World;

namespace Mocha.Renderer;

public class ModelSceneObject : SceneObject
{
	public List<Model> models;

	public Matrix4x4 ModelMatrix
	{
		get
		{
			var matrix = Matrix4x4.CreateFromQuaternion( Entity.Transform.Rotation.GetSystemQuaternion() );
			matrix *= Matrix4x4.CreateScale( Entity.Transform.Scale );
			matrix *= Matrix4x4.CreateTranslation( Entity.Transform.Position );
			return matrix;
		}
	}

	public ModelSceneObject( IEntity entity ) : base( entity )
	{
	}

	public void SetModels( List<Model> models )
	{
		this.models = models;
	}
}
