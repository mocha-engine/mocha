using System.Text.Json;

namespace Mocha.Networking;
internal static class NetworkSerializer
{
	public static byte[] Serialize( object obj )
	{
		return JsonSerializer.SerializeToUtf8Bytes( obj );
	}
}
