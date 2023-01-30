using System.Reflection;

namespace Mocha.Hotload;

/// <summary>
/// A wrapper class for property and field infos.
/// </summary>
internal sealed class UpgradableMember
{
	/// <summary>
	/// The name of the member.
	/// </summary>
	internal string Name { get; }

	/// <summary>
	/// The type that this member contains.
	/// </summary>
	internal Type Type { get; }

	// In object: instance
	// Out object: value
	private readonly Func<object, object?> _getter;

	// In object: instance
	// In object: value
	private readonly Action<object, object> _setter;

	private UpgradableMember( Func<object, object> getter, Action<object, object> setter, Type type, string name )
	{
		_getter = getter;
		_setter = setter;

		Type = type;
		Name = name;
	}

	/// <summary>
	/// Set the value that this member represents.
	/// This will bail if <see cref="Type"/> is not assignable from <paramref name="value"/>.
	/// </summary>
	internal void SetValue( object instance, object value )
	{
		if ( !Type.IsAssignableFrom( value.GetType() ) )
		{
			// Bail
			return;
		}

		_setter?.Invoke( instance, value );
	}

	/// <summary>
	/// Get the value that this member represents.
	/// </summary>
	internal object? GetValue( object instance )
	{
		return _getter?.Invoke( instance );
	}

	#region "Constructors"

	/// <summary>
	/// Create an <see cref="UpgradableMember"/> given a member.
	/// This will internally call <see cref="FromProperty(PropertyInfo)"/> or
	/// <see cref="FromField(FieldInfo)"/> depending on the member type.
	/// Null is returned if this cannot be made into an UpgradableMember.
	/// </summary>
	internal static UpgradableMember? FromMember( MemberInfo memberInfo )
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
	internal static UpgradableMember FromField( FieldInfo fieldInfo )
	{
		return new( fieldInfo.GetValue, fieldInfo.SetValue, fieldInfo.FieldType, fieldInfo.Name );
	}

	/// <summary>
	/// Create an <see cref="UpgradableMember"/> given a property
	/// </summary>
	internal static UpgradableMember? FromProperty( PropertyInfo propertyInfo )
	{
		var getMethod = propertyInfo.GetGetMethod( true );
		var setMethod = propertyInfo.GetSetMethod( true );

		if ( getMethod is null )
			return null;

		if ( setMethod is null )
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

		return new( invokeGet, invokeSet, propertyInfo.PropertyType, propertyInfo.Name );
	}
	#endregion
}
