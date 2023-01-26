using System.Collections;

namespace Mocha.Common;

public sealed class EntityRegistry : IEnumerable<IEntity>
{
	private static EntityRegistry? s_instance;
	public static EntityRegistry Instance
	{
		get
		{
			s_instance ??= new();
			return s_instance;
		}
	}

	private readonly List<IEntity> _entities = new();

	public void RegisterEntity( IEntity entity )
	{
		// Don't add duplicates
		if ( _entities.Contains( entity ) )
			return;

		_entities.Add( entity );
	}

	public void UnregisterEntity( IEntity entity )
	{
		_entities.Remove( entity );
	}

	public IEnumerator<IEntity> GetEnumerator() => _entities.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _entities.GetEnumerator();

	public IEntity this[int index]
	{
		get => _entities.ElementAt( index );
		set => _entities[index] = value;
	}
}
