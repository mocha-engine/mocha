namespace Mocha.Networking;

public class ArrayConverter : NetConverter<Array>
{
	public void Serialize( Array value, BinaryWriter binaryWriter )
	{
		// Get the type of the array.
		Type type = value.GetType().GetElementType();

		// Write the type of the array.
		binaryWriter.Write( type.FullName );
		binaryWriter.Write( value.Length );
		foreach ( var item in value )
		{
			var converter = NetworkSerializer.GetConverterForType( item.GetType() );

			if ( converter != null )
				converter.Serialize( (dynamic)item, binaryWriter );
			else
				throw new Exception( "No converter found for type " + item.GetType() );
		}
	}

	public Array Deserialize( BinaryReader binaryReader )
	{
		// Read the type of the array.
		var typeName = binaryReader.ReadString();
		Type type = Type.GetType( typeName )!;

		// Read the length of the array.
		var length = binaryReader.ReadInt32();
		var array = Array.CreateInstance( type, length );
		for ( var i = 0; i < length; i++ )
		{
			var converter = NetworkSerializer.GetConverterForType( type );

			if ( converter != null )
				array.SetValue( converter.Deserialize( binaryReader ), i );
			else
				throw new Exception( "No converter found for type " + typeName );
		}

		return array;
	}
}
