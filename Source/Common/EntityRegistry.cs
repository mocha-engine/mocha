using System.Collections;

namespace Mocha.Common;

public sealed class EntityRegistry : IEnumerable<IEntity>
{
	private static EntityRegistry? instance;
	public static EntityRegistry Instance
	{
		get
		{
			instance ??= new();
			return instance;
		}
	}

	private readonly List<IEntity> Entities = new();

	public void RegisterEntity( IEntity entity )
	{
		// Don't add duplicates
		if ( Entities.Contains( entity ) )
			return;

		Entities.Add( entity );
	}

	public void UnregisterEntity( IEntity entity )
	{
		Entities.Remove( entity );
	}

	public IEnumerator<IEntity> GetEnumerator() => Entities.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => Entities.GetEnumerator();

	public IEntity this[int index]
	{
		get => Entities.ElementAt( index );
		set => Entities[index] = value;
	}
}
