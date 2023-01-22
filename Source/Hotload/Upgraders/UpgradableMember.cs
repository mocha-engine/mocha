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

	/// <summary>
	/// Set the value that this member represents
	/// </summary>
	public void SetValue( object instance, object value )
	{
		Setter?.Invoke( instance, value );
	}

	/// <summary>
	/// Get the value that this member represents
	/// </summary>
	public object? GetValue( object instance )
	{
		return Getter?.Invoke( instance );
	}

	#region "Constructors"

	/// <summary>
	/// Create an <see cref="UpgradableMember"/> given a member.
	/// This will internally call <see cref="FromProperty(PropertyInfo)"/> or
	/// <see cref="FromField(FieldInfo)"/> depending on the member type.
	/// Null is returned if this cannot be made into an UpgradableMember.
	/// </summary>
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

	/// <summary>
	/// Create an <see cref="UpgradableMember"/> given a field
	/// </summary>
	public static UpgradableMember FromField( FieldInfo fieldInfo )
	{
		return new( fieldInfo.GetValue, fieldInfo.SetValue, fieldInfo.FieldType );
	}

	/// <summary>
	/// Create an <see cref="UpgradableMember"/> given a property
	/// </summary>
	public static UpgradableMember? FromProperty( PropertyInfo propertyInfo )
	{
		var getMethod = propertyInfo.GetGetMethod( true );
		var setMethod = propertyInfo.GetSetMethod( true );

		if ( getMethod == null )
			return null;

		if ( setMethod == null )
			return null;

		// Some get methods (array indexers) have parameters which we don't support
		// yet. We can't upgrade these.
		if ( getMethod.GetParameters().Length != 0 )
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
