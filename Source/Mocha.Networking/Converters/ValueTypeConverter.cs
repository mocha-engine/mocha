using System.Reflection;

namespace Mocha.Networking;
public class ValueTypeConverter : NetConverter<ValueType>
{
	public void Serialize( ValueType value, BinaryWriter binaryWriter )
	{
		// Serialize every field or property in the value type marked with [Replicated]

		// Write type name
		binaryWriter.Write( value.GetType().FullName );

		foreach ( var member in value.GetType().GetMembers() )
		{
			if ( member.MemberType == MemberTypes.Field )
			{
				var field = (FieldInfo)member;
				if ( field.IsDefined( typeof( ReplicatedAttribute ), true ) )
				{
					var bytes = NetworkSerializer.Serialize( field.GetValue( value ) );

					binaryWriter.Write( bytes.Length );
					binaryWriter.Write( bytes );
				}
			}
			else if ( member.MemberType == MemberTypes.Property )
			{
				var property = (PropertyInfo)member;
				if ( property.IsDefined( typeof( ReplicatedAttribute ), true ) )
				{
					var bytes = NetworkSerializer.Serialize( property.GetValue( value ) );

					binaryWriter.Write( bytes.Length );
					binaryWriter.Write( bytes );
				}
			}
		}
	}

	public ValueType Deserialize( BinaryReader binaryReader )
	{
		// Deserialize every field or property in the value type marked with [Replicated]

		// Read type name
		var typeName = binaryReader.ReadString();

		// Create instance of type
		var type = Type.GetType( typeName )!;
		var instance = Activator.CreateInstance( type );

		foreach ( var member in type.GetMembers() )
		{
			if ( member.MemberType == MemberTypes.Field )
			{
				var field = (FieldInfo)member;
				if ( field.IsDefined( typeof( ReplicatedAttribute ), true ) )
				{
					var length = binaryReader.ReadInt32();
					var bytes = binaryReader.ReadBytes( length );

					field.SetValue( instance, NetworkSerializer.Deserialize( bytes, field.FieldType ) );
				}
			}
			else if ( member.MemberType == MemberTypes.Property )
			{
				var property = (PropertyInfo)member;
				if ( property.IsDefined( typeof( ReplicatedAttribute ), true ) )
				{
					var length = binaryReader.ReadInt32();
					var bytes = binaryReader.ReadBytes( length );

					property.SetValue( instance, NetworkSerializer.Deserialize( bytes, property.PropertyType ) );
				}
			}
		}

		return (ValueType)instance;
	}
}
