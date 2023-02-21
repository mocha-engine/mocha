namespace Mocha;

public class NetString : INetType<string>
{
	/// <summary>
	/// Backing value
	/// </summary>
	public string Value { get; set; }

	private NetString( string value )
	{
		Value = value;
	}

	//
	// Implicit conversions
	//
	public static implicit operator string( NetString netString )
	{
		return netString.Value;
	}

	public static implicit operator NetString( string value )
	{
		return new NetString( value );
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
		Value = binaryReader.ReadString();
	}
}
