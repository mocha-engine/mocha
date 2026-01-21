using MessagePack;
using MessagePack.Formatters;
using Mocha.Common;

namespace Mocha.Networking;

internal class MochaResolver : IFormatterResolver
{
	// Resolver should be singleton.
	public static readonly IFormatterResolver Instance = new MochaResolver();

	private MochaResolver()
	{
	}

	// GetFormatter<T>'s get cost should be minimized so use type cache.
	public IMessagePackFormatter<T> GetFormatter<T>()
	{
		return FormatterCache<T>.Formatter;
	}

	private static class FormatterCache<T>
	{
		public static readonly IMessagePackFormatter<T> Formatter;

		// generic's static constructor should be minimized for reduce type generation size!
		// use outer helper method.
		static FormatterCache()
		{
			Formatter = (IMessagePackFormatter<T>)SampleCustomResolverGetFormatterHelper.GetFormatter( typeof( T ) );
		}
	}
}

internal static class SampleCustomResolverGetFormatterHelper
{
	static readonly Dictionary<Type, object> formatterMap = new Dictionary<Type, object>()
	{
		{ typeof( Vector3 ), new Vector3Formatter() },
		{ typeof( Rotation ), new RotationFormatter() },
		{ typeof( NetworkId ), new NetworkIdFormatter() },
	};

	internal static object GetFormatter( Type t )
	{
		object formatter;
		if ( formatterMap.TryGetValue( t, out formatter ) )
		{
			return formatter;
		}

		// If type can not get, must return null for fallback mechanism.
		return null;
	}
}
