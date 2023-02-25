using Mocha.Common;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Mocha.Networking;

internal static class NetworkSerializer
{
	private const bool UseCompression = false;

	// TODO: Use reflection here
	private static readonly Dictionary<Type, object> s_converters = new()
	{
		{ typeof( string ), new StringConverter() },
		{ typeof( float ), new FloatConverter() },
		{ typeof( byte) , new ByteConverter() },
		{ typeof( int ), new IntConverter() },

		{ typeof( Vector3 ), new Vector3Converter() },
		{ typeof( Array ), new ArrayConverter() },
		{ typeof( IList ), new ListConverter() },
		{ typeof( ValueType ), new ValueTypeConverter() }
	};

	private static object? GetMemberValue( object owner, MemberInfo memberInfo )
	{
		switch ( memberInfo )
		{
			case FieldInfo fieldInfo:
				return fieldInfo.GetValue( owner );
			case PropertyInfo propertyInfo:
				return propertyInfo.GetValue( owner );
		}

		return null;
	}

	private static void SetMemberValue( object owner, MemberInfo memberInfo, object value )
	{
		switch ( memberInfo )
		{
			case PropertyInfo propertyInfo:
				propertyInfo.SetValue( owner, value );
				break;
			case FieldInfo fieldInfo:
				fieldInfo.SetValue( owner, value );
				break;
			default:
				return;
		}
	}

	private static Type? GetMemberType( MemberInfo memberInfo )
	{
		return memberInfo switch
		{
			PropertyInfo propertyInfo => propertyInfo.PropertyType,
			FieldInfo fieldInfo => fieldInfo.FieldType,
			_ => null
		};
	}

	private static object GetDefaultValue( Type type )
	{
		return type.IsValueType ? Activator.CreateInstance( type ) : null!;
	}

	internal static dynamic? GetConverterForType( Type type )
	{
		// Find a converter where the type is the same as the type we're looking for
		// or derives from it

		foreach ( var converter in s_converters )
		{
			if ( converter.Key.IsAssignableFrom( type ) )
				return converter.Value;
		}

		return null!;
	}

	internal static int? GetIndexForType( Type type )
	{
		var index = 0;

		foreach ( var converter in s_converters )
		{
			if ( converter.Key.IsAssignableFrom( type ) )
				return index;

			index++;
		}

		return null;
	}

	private static byte[] SerializeMember( object owner, MemberInfo memberInfo )
	{
		// If this member does not have the [Replicated] attribute, bail
		if ( !memberInfo.GetCustomAttributes( typeof( ReplicatedAttribute ), true ).Any() )
			return new byte[0];

		var value = GetMemberValue( owner, memberInfo );

		if ( value == null )
		{
			throw new SerializationException( $"No value?" );
		}

		using var memoryStream = new MemoryStream();
		using var binaryWriter = new BinaryWriter( memoryStream );

		var type = value.GetType();
		dynamic? converter = GetConverterForType( type );

		if ( converter == null )
		{
			throw new SerializationException( $"No handler for {type.Name}" );
		}

		// Call Serialize on the converter.
		// Serialize will inspect ( T value, BinaryWriter binaryWriter ),
		// but we have object value, so we need to cast it to T.
		converter.Serialize( (dynamic)value, binaryWriter );

		return memoryStream.ToArray();
	}

	private static void DeserializeMember( object obj, MemberInfo memberInfo, byte[] data )
	{
		// If this member does not have the [Replicated] attribute, bail
		if ( !memberInfo.GetCustomAttributes( typeof( ReplicatedAttribute ), true ).Any() )
			return;

		var memberType = GetMemberType( memberInfo );

		if ( memberType == null )
			return;

		var value = GetDefaultValue( memberType );

		if ( data.Length > 0 )
		{
			dynamic? converter = GetConverterForType( memberType );

			if ( converter == null )
				return; // Skip

			using var memoryStream = new MemoryStream( data );
			using var binaryReader = new BinaryReader( memoryStream );

			// Call Deserialize on the converter.
			// Deserialize will inspect ( BinaryReader binaryReader ),
			// but we have object value, so we need to cast it to T.
			value = converter.Deserialize( binaryReader );
		}

		SetMemberValue( obj, memberInfo, value );
	}

	private static object DeserializeValue( Type type, byte[] data )
	{
		dynamic? converter = GetConverterForType( type );

		if ( converter == null )
			return GetDefaultValue( type );

		using var memoryStream = new MemoryStream( data );
		using var binaryReader = new BinaryReader( memoryStream );

		// Call Deserialize on the converter.
		// Deserialize will inspect ( BinaryReader binaryReader ),
		// but we have object value, so we need to cast it to T.
		return converter.Deserialize( binaryReader );
	}

	public static byte[] Serialize( object obj )
	{
		using var memoryStream = new MemoryStream();
		using var binaryWriter = new BinaryWriter( memoryStream );

		foreach ( var memberInfo in obj.GetType().GetMembers() )
		{
			if ( memberInfo.MemberType != MemberTypes.Field && memberInfo.MemberType != MemberTypes.Property )
				continue;

			// Serialize the member name
			binaryWriter.Write( memberInfo.Name );

			// Serialize the member value
			var data = SerializeMember( obj, memberInfo );
			binaryWriter.Write( data.Length );
			binaryWriter.Write( data );
		}

		var bytes = memoryStream.ToArray();

		// Debug
		{
			var dump = HexDump.Dump( bytes, 16 );
			Log.Info( "Dump:\n" + dump );
		}

		return UseCompression ? Serializer.Compress( bytes ) : bytes;
	}

	public static T Deserialize<T>( byte[] data )
	{
		data = UseCompression ? Serializer.Decompress( data ) : data;

		using var memoryStream = new MemoryStream( data );
		using var binaryReader = new BinaryReader( memoryStream );

		var type = typeof( T );
		object obj;

		if ( type.IsArray )
		{
			var elementType = type.GetElementType();
			var length = binaryReader.ReadInt32();
			obj = Array.CreateInstance( elementType, length );

			for ( int i = 0; i < length; i++ )
			{
				var memberData = binaryReader.ReadBytes( Marshal.SizeOf( elementType ) );
				var value = DeserializeValue( elementType, memberData );
				((Array)obj).SetValue( value, i );
			}
		}
		else
		{
			obj = Activator.CreateInstance( type );

			while ( memoryStream.Position < memoryStream.Length )
			{
				var memberName = binaryReader.ReadString();
				var length = binaryReader.ReadInt32();
				var memberData = binaryReader.ReadBytes( length );

				var member = type.GetMember( memberName )[0];
				DeserializeMember( obj, member, memberData );
			}
		}

		return (T)obj;
	}

	public static object? Deserialize( byte[] data, Type type )
	{
		data = UseCompression ? Serializer.Decompress( data ) : data;

		using var memoryStream = new MemoryStream( data );
		using var binaryReader = new BinaryReader( memoryStream );

		object obj;

		if ( type == typeof( string ) )
			obj = "";
		else
			obj = FormatterServices.GetUninitializedObject( type );

		while ( memoryStream.Position < memoryStream.Length )
		{
			var memberName = binaryReader.ReadString();
			var length = binaryReader.ReadInt32();
			var memberData = binaryReader.ReadBytes( length );

			var member = type.GetMember( memberName )[0];
			DeserializeMember( obj, member, memberData );
		}

		return obj;
	}

}
