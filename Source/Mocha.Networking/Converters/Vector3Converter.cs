using Mocha.Common;

namespace Mocha.Networking;

public class Vector3Converter : NetConverter<Vector3>
{
	public void Serialize( Vector3 value, BinaryWriter binaryWriter )
	{
		binaryWriter.Write( value.X );
		binaryWriter.Write( value.Y );
		binaryWriter.Write( value.Z );
	}

	public Vector3 Deserialize( BinaryReader binaryReader )
	{
		var x = binaryReader.ReadSingle();
		var y = binaryReader.ReadSingle();
		var z = binaryReader.ReadSingle();

		return new Vector3( x, y, z );
	}
}
