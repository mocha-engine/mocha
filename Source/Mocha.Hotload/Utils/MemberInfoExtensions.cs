using System.Collections;
using System.Reflection;

namespace Mocha.Hotload.Util;

/// <summary>
/// A collection of extension methods for a <see cref="MemberInfo"/>.
/// </summary>
internal static class MemberInfoExtensions
{
	/// <summary>
	/// Returns whether or not the <see cref="MemberInfo"/>s type is a class.
	/// </summary>
	/// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
	/// <returns>Whether or not the <see cref="MemberInfo"/>s type is a class.</returns>
	internal static bool IsTypeClass( this MemberInfo memberInfo ) => memberInfo switch
	{
		// Return is class & is not delegate & isn't/doesn't derive from dictionary or list
		PropertyInfo propertyInfo => propertyInfo.PropertyType.IsClass
				&& !propertyInfo.PropertyType.IsSubclassOf( typeof( Delegate ) )
				&& !propertyInfo.PropertyType.IsArray
				&& !propertyInfo.PropertyType.IsSubclassOf( typeof( ICollection<> ) ),
		FieldInfo fieldInfo => fieldInfo.FieldType.IsClass
				&& !fieldInfo.FieldType.IsSubclassOf( typeof( Delegate ) )
				&& !fieldInfo.FieldType.IsArray
				&& !fieldInfo.FieldType.IsSubclassOf( typeof( ICollection<> ) ),
		_ => false
	};

	/// <summary>
	/// Returns whether or not the <see cref="MemberInfo"/>s type is a struct.
	/// </summary>
	/// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
	/// <returns>Whether or not the <see cref="MemberInfo"/>s type is a struct.</returns>
	internal static bool IsTypeStruct( this MemberInfo memberInfo ) => memberInfo switch
	{
		PropertyInfo propertyInfo => propertyInfo.PropertyType.IsValueType && !propertyInfo.PropertyType.IsEnum,
		FieldInfo fieldInfo => fieldInfo.FieldType.IsValueType && !fieldInfo.FieldType.IsEnum,
		_ => false
	};

	/// <summary>
	/// Returns whether or not the <see cref="MemberInfo"/>s type is a <see cref="ICollection"/>.
	/// </summary>
	/// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
	/// <returns>Whether or not the <see cref="MemberInfo"/>s type is a <see cref="ICollection"/>.</returns>
	internal static bool IsTypeCollection( this MemberInfo memberInfo ) => memberInfo switch
	{
		PropertyInfo propertyInfo => propertyInfo.PropertyType.GetInterface( nameof( ICollection ) ) is not null,
		FieldInfo fieldInfo => fieldInfo.FieldType.GetInterface( nameof( ICollection ) ) is not null,
		_ => false
	};

	/// <summary>
	/// Returns whether or not the <see cref="MemberInfo"/>s type is an interface.
	/// </summary>
	/// <param name="memberInfo">The <see cref="MemberInfo"/> to check.</param>
	/// <returns>Whether or not the <see cref="MemberInfo"/>s type is an interface.</returns>
	internal static bool IsTypeInterface( this MemberInfo memberInfo ) => memberInfo switch
	{
		PropertyInfo propertyInfo => propertyInfo.PropertyType.IsInterface,
		FieldInfo fieldInfo => fieldInfo.FieldType.IsInterface,
		_ => false
	};
}
