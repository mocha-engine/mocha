using System.Reflection;

namespace Mocha.Common.World;

public interface IEntity
{
	public static List<IEntity> All => Assembly.GetCallingAssembly().GetTypes().OfType<IEntity>().ToList();

	public int Id { get; set; }
	public Transform Transform { get; set; }

	void Delete( bool immediate );
}
