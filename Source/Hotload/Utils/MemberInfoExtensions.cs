using System.Reflection;

namespace Mocha.Hotload;

internal static class MemberInfoExtensions
{
	internal static bool IsClass( this MemberInfo memberInfo )
	{
		if ( memberInfo is PropertyInfo propertyInfo )
		{
			// Return is class & is not delegate & isn't/doesn't derive from dictionary or list

			return propertyInfo.PropertyType.IsClass
				&& !propertyInfo.PropertyType.IsSubclassOf( typeof( Delegate ) )
				&& !propertyInfo.PropertyType.IsArray
				&& !propertyInfo.PropertyType.IsSubclassOf( typeof( ICollection<> ) );
		}
		else if ( memberInfo is FieldInfo fieldInfo )
		{
			return fieldInfo.FieldType.IsClass
				&& !fieldInfo.FieldType.IsSubclassOf( typeof( Delegate ) )
				&& !fieldInfo.FieldType.IsArray
				&& !fieldInfo.FieldType.IsSubclassOf( typeof( ICollection<> ) );
		}

		return false;
	}
}
