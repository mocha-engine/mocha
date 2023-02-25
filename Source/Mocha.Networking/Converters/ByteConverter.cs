namespace Mocha.Networking;

public class ByteConverter : NetConverter<byte>
{
	public void Serialize( byte value, BinaryWriter binaryWriter )
	{
		binaryWriter.Write( value );
	}

	public byte Deserialize( BinaryReader binaryReader )
	{
		return binaryReader.ReadByte();
	}
}
