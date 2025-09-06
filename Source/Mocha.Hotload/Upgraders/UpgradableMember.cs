using System.Reflection;

namespace Mocha.Hotload.Upgrading;

/// <summary>
/// A wrapper class for <see cref="PropertyInfo"/> and <see cref="FieldInfo"/>.
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

	/// <summary>
	/// Whether or not the member is static.
	/// </summary>
	internal bool Static { get; }

	/// <summary>
	/// In: instance.
	/// Out: Value.
	/// </summary>
	private readonly Func<object?, object?> _getter;

	/// <summary>
	/// In: instance, value.
	/// </summary>
	private readonly Action<object?, object?> _setter;

	/// <summary>
	/// Initializes a new instance of <see cref="UpgradableMember"/>.
	/// </summary>
	/// <param name="name">The name of the member.</param>
	/// <param name="type">The type of the member.</param>
	/// <param name="isStatic">Whether or not the member is static.</param>
	/// <param name="getter">The getter method for the member.</param>
	/// <param name="setter">The setter method for the member.</param>
	private UpgradableMember( string name, Type type, bool isStatic, Func<object?, object?> getter, Action<object?, object?> setter )
	{
		Name = name;
		Type = type;
		Static = isStatic;

		_getter = getter;
		_setter = setter;
	}

	/// <summary>
	/// Sets the value that this member represents. This will bail if <see cref="Type"/> is not assignable from <paramref name="value"/>.
	/// </summary>
	internal void SetValue( object? instance, object? value )
	{
		if ( !Type.IsAssignableFrom( value?.GetType() ) )
			// Bail
			return;

		_setter.Invoke( instance, value );
	}

	/// <summary>
	/// Gets the value that this member represents.
	/// </summary>
	internal object? GetValue( object? instance )
	{
		return _getter.Invoke( instance );
	}

	#region "Constructors"
	/// <summary>
	/// Create an <see cref="UpgradableMember"/> given a member.
	/// This will internally call <see cref="FromProperty(PropertyInfo)"/> or <see cref="FromField(FieldInfo)"/> depending on the member type.
	/// Null is returned if this cannot be made into an <see cref="UpgradableMember"/>.
	/// </summary>
	internal static UpgradableMember? FromMember( MemberInfo memberInfo )
	{
		if ( memberInfo is PropertyInfo propertyInfo )
			return FromProperty( propertyInfo );
		else if ( memberInfo is FieldInfo fieldInfo && !fieldInfo.IsInitOnly )
			return FromField( fieldInfo );

		// Can't upgrade this, so return null.
		return null;
	}

	/// <summary>
	/// Creates an <see cref="UpgradableMember"/> given a <see cref="FieldInfo"/>.
	/// </summary>
	/// <returns>A <see cref="UpgradableMember"/> that represents the <see cref="FieldInfo"/>.</returns>
	internal static UpgradableMember FromField( FieldInfo fieldInfo )
	{
		return new( fieldInfo.Name, fieldInfo.FieldType, fieldInfo.IsStatic, fieldInfo.GetValue, fieldInfo.SetValue );
	}

	/// <summary>
	/// Creates an <see cref="UpgradableMember"/> given a <see cref="PropertyInfo"/>.
	/// If the <see cref="PropertyInfo"/> does not have a getter, a setter, or the getter has parameters then null is returned.
	/// </summary>
	/// <returns>A <see cref="UpgradableMember"/> that represents the <see cref="PropertyInfo"/>. Null is returned if the <see cref="PropertyInfo"/> does not satisfy the conditions outlined.</returns>
	internal static UpgradableMember? FromProperty( PropertyInfo propertyInfo )
	{
		var getMethod = propertyInfo.GetGetMethod( true );
		var setMethod = propertyInfo.GetSetMethod( true );

		if ( getMethod is null || setMethod is null )
			return null;

		// TODO: Some get methods (array indexers) have parameters which we don't support
		// yet. We can't upgrade these.
		if ( getMethod.GetParameters().Length != 0 )
			return null;

		var invokeGet = ( object? instance ) => getMethod.Invoke( instance, null );
		var invokeSet = ( object? instance, object? value ) =>
		{
			setMethod.Invoke( instance, new[] { value } );
		};

		return new( propertyInfo.Name, propertyInfo.PropertyType, getMethod.IsStatic, invokeGet, invokeSet );
	}
	#endregion
}
