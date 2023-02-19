namespace Mocha.Networking;

public interface INetType<T> where T : notnull
{
	T Value { get; set; }

	// TODO: NetWriter/NetReader?
	byte[] Serialize();
	void Deserialize( byte[] values );
}
