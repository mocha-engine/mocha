using System.Collections;

namespace Mocha.Common;

public sealed class EntityRegistry : IEnumerable<IActor>
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

	private readonly List<IActor> _entities = new();

	public void RegisterEntity( IActor entity )
	{
		// Don't add duplicates
		if ( _entities.Contains( entity ) )
			return;

		_entities.Add( entity );
	}

	public void UnregisterEntity( IActor entity )
	{
		_entities.Remove( entity );
	}

	public IEnumerator<IActor> GetEnumerator() => _entities.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _entities.GetEnumerator();

	public IActor this[int index]
	{
		get => _entities.ElementAt( index );
		set => _entities[index] = value;
	}
}
