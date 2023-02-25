namespace Mocha.Networking;

public interface NetConverter<T> where T : notnull
{
	abstract void Serialize( T value, BinaryWriter binaryWriter );
	abstract T Deserialize( BinaryReader binaryReader );
}
