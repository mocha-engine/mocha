namespace Mocha.Networking;

public class IntConverter : NetConverter<int>
{
	public void Serialize( int value, BinaryWriter binaryWriter )
	{
		binaryWriter.Write( value );
	}

	public int Deserialize( BinaryReader binaryReader )
	{
		return binaryReader.ReadInt32();
	}
}
