using Mocha.Common.World;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha.Renderer;

public class SceneObject
{
	public IEntity Entity { get; set; }
	public Transform Transform => Entity.Transform;
	public static List<SceneObject> All { get; set; } = Assembly.GetCallingAssembly().GetTypes().OfType<SceneObject>().ToList();

	public SceneObject( IEntity entity )
	{
		All.Add( this );
		Entity = entity;
	}

	public virtual void Delete() { }

	public bool Equals( SceneObject x, SceneObject y ) => x.GetHashCode() == y.GetHashCode();
	public int GetHashCode( [DisallowNull] SceneObject obj ) => base.GetHashCode();
}
