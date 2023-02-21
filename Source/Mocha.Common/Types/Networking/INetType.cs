namespace Mocha;

public interface INetType<T> where T : notnull
{
	T Value { get; set; }

	void Serialize( BinaryWriter binaryWriter );
	void Deserialize( BinaryReader binaryReader );
}
