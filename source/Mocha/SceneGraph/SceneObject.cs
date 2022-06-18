using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha.Renderer;

public class SceneObject
{
	public static List<SceneObject> All { get; set; } = Assembly.GetCallingAssembly().GetTypes().OfType<SceneObject>().ToList();

	public IEntity Entity { get; set; }

	public Matrix4x4 ModelMatrix
	{
		get
		{
			var matrix = Matrix4x4.CreateScale( Entity.Transform.Scale );
			matrix *= Matrix4x4.CreateTranslation( Entity.Transform.Position );
			matrix *= Matrix4x4.CreateFromQuaternion( Entity.Transform.Rotation.GetSystemQuaternion() );
			return matrix;
		}
	}

	public SceneObject( IEntity entity )
	{
		All.Add( this );
		Entity = entity;
	}

	public virtual void Render( CommandList commandList ) { }
	public virtual void Update() { }

	public virtual void Delete() { }

	public bool Equals( SceneObject x, SceneObject y ) => x.GetHashCode() == y.GetHashCode();
	public int GetHashCode( [DisallowNull] SceneObject obj ) => base.GetHashCode();
}
