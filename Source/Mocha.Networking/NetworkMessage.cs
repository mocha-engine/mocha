using System.Text.Json;

namespace Mocha.Networking;

public class NetworkMessageWrapper<T>
{
	public int NetworkMessageType { get; set; } = -1;
	public T Data { get; set; } = default!;

	public virtual byte[] Serialize()
	{
		return JsonSerializer.SerializeToUtf8Bytes( this );
	}
}
