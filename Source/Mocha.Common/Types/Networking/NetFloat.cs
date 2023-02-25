namespace Mocha;

[Obsolete]
[HandlesNetworkedType<float>]
public class NetFloat : INetType<float>
{
	/// <summary>
	/// Backing value
	/// </summary>
	public float Value { get; set; }

	private NetFloat( float value )
	{
		Value = value;
	}

	//
	// Implicit conversions
	//
	public static implicit operator float( NetFloat netString )
	{
		return netString.Value;
	}

	public static implicit operator NetFloat( float value )
	{
		return new NetFloat( value );
	}

	//
	// Serialization functions
	//
	public void Serialize( BinaryWriter binaryWriter )
	{
		binaryWriter.Write( Value );
	}

	public void Deserialize( BinaryReader binaryReader )
	{
		Value = binaryReader.ReadSingle();
	}
}
