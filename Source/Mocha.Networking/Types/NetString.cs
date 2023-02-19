using System.Text;

namespace Mocha.Networking;

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
	// Implicit conversions to/from string
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
	public byte[] Serialize()
	{
		return Encoding.UTF8.GetBytes( Value );
	}

	public void Deserialize( byte[] values )
	{
		Value = Encoding.UTF8.GetString( values );
	}
}
