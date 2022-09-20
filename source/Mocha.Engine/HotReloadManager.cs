using Mocha.Engine;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler( typeof( HotReloadManager ) )]

namespace Mocha.Engine;

internal static class HotReloadManager
{
	public static void ClearCache( Type[]? types )
	{
	}

	public static void UpdateApplication( Type[]? types )
	{
		Event.Run( Event.HotloadAttribute.Name );
	}
}
