using System.Reflection;

namespace Mocha.Hotload;

public class UpgradableMember
{
	// In object: instance
	// Out object: value
	private Func<object, object?> Getter;

	// In object: instance
	// In object: value
	private Action<object, object> Setter;

	public Type Type { get; private set; }

	private UpgradableMember( Func<object, object> getter, Action<object, object> setter, Type type )
	{
		Getter = getter;
		Setter = setter;
		Type = type;
	}

	public void SetValue( object instance, object value )
	{
		Setter?.Invoke( instance, value );
	}

	public object? GetValue( object instance )
	{
		return Getter?.Invoke( instance );
	}

	#region "Constructors"
	public static UpgradableMember? FromMember( MemberInfo memberInfo )
	{
		if ( memberInfo is PropertyInfo propertyInfo )
		{
			return FromProperty( propertyInfo );
		}
		else if ( memberInfo is FieldInfo fieldInfo )
		{
			return FromField( fieldInfo );
		}

		// Can't upgrade this, so don't return an UpgradableMember
		return null;
	}

	public static UpgradableMember FromField( FieldInfo fieldInfo )
	{
		return new( fieldInfo.GetValue, fieldInfo.SetValue, fieldInfo.FieldType );
	}

	public static UpgradableMember? FromProperty( PropertyInfo propertyInfo )
	{
		var getMethod = propertyInfo.GetGetMethod( true );
		var setMethod = propertyInfo.GetSetMethod( true );

		if ( getMethod == null )
			return null;

		if ( setMethod == null )
			return null;

		var invokeGet = ( object instance ) => getMethod.Invoke( instance, null );
		var invokeSet = ( object instance, object value ) =>
		{
			setMethod.Invoke( instance, new[] { value } );
		};

		return new( invokeGet, invokeSet, propertyInfo.PropertyType );
	}
	#endregion
}
