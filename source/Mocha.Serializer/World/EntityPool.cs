namespace Mocha.Common.World;

public static class EntityPool
{
	private static List<IEntity> entities = new();
	public static IReadOnlyList<IEntity> Entities => entities.AsReadOnly();

	public static int RegisterEntity( IEntity entity )
	{
		entities.Add( entity );
		return entities.Count;
	}

	public static void DeleteEntity( IEntity entity )
	{
		entities.Remove( entity );
	}
}
