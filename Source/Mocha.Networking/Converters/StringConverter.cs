namespace Mocha.Networking;

public class StringConverter : NetConverter<string>
{
	public void Serialize( string value, BinaryWriter binaryWriter )
	{
		binaryWriter.Write( value );
	}

	public string Deserialize( BinaryReader binaryReader )
	{
		return binaryReader.ReadString();
	}
}
