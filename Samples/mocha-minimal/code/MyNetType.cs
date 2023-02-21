using System.IO;

namespace Minimal;

public record struct MyType( float X, float Y, float Z );

public class MyNetType : INetType<MyType>
{
	public MyType Value { get; set; }

	public void Deserialize( BinaryReader binaryReader )
	{
		var value = new MyType();
		value.X = binaryReader.ReadSingle();
		value.Y = binaryReader.ReadSingle();
		value.Z = binaryReader.ReadSingle();
	}

	public void Serialize( BinaryWriter binaryWriter )
	{
		binaryWriter.Write( Value.X );
		binaryWriter.Write( Value.Y );
		binaryWriter.Write( Value.Z );
	}
}
