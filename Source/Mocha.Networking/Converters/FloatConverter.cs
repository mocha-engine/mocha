namespace Mocha.Networking;

public class FloatConverter : NetConverter<float>
{
	public void Serialize( float value, BinaryWriter binaryWriter )
	{
		binaryWriter.Write( value );
	}

	public float Deserialize( BinaryReader binaryReader )
	{
		return binaryReader.ReadSingle();
	}
}
