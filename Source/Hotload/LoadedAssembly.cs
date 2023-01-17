using System.Reflection;

namespace Mocha;

public class LoadedAssemblyType<T>
{
	private T? ManagedClass { get; }

	public LoadedAssemblyType( string dllPath )
	{
		var name = AssemblyName.GetAssemblyName( dllPath );
		var currentAssembly = AppDomain.CurrentDomain.Load( name );

		// Find first type that derives from T
		foreach ( var type in currentAssembly.GetTypes() )
		{
			if ( type.GetInterface( typeof( T ).FullName! ) != null )
			{
				ManagedClass = (T)Activator.CreateInstance( type )!;
				break;
			}
		}

		if ( ManagedClass == null )
		{
			throw new Exception( "Could not find IGame implementation" );
		}
	}

	public T Value => ManagedClass!;

	public static implicit operator T( LoadedAssemblyType<T> loadedAssemblyType )
	{
		return loadedAssemblyType.ManagedClass!;
	}
}
