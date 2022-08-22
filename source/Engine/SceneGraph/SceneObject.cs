using Mocha.Common.World;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha.Renderer;

public class SceneObject
{
	public Transform Transform { get; set; }
	public static List<SceneObject> All { get; set; } = Assembly.GetCallingAssembly().GetTypes().OfType<SceneObject>().ToList();
	public bool IsVisible { get; set; } = true;

	public SceneObject()
	{
		All.Add( this );
	}

	public virtual void Delete() { }
	public virtual void Render() { }

	public bool Equals( SceneObject x, SceneObject y ) => x.GetHashCode() == y.GetHashCode();
	public int GetHashCode( [DisallowNull] SceneObject obj ) => base.GetHashCode();
}
