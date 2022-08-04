using System.ComponentModel;
using System.Reflection;

namespace Mocha.Common;

public class DisplayInfo
{
	public string Name { get; set; } = "";
	public string Category { get; set; } = "";
	public string ImageIcon { get; set; } = "";
	public string TextIcon { get; set; } = "";

	public string CombinedTitle => $"{TextIcon} {Name}";

	public static DisplayInfo For( object obj )
	{
		var type = obj.GetType();

		return For( type );
	}

	public static DisplayInfo For( Type type )
	{
		return new DisplayInfo()
		{
			Name = GetTypeTitle( type ),
			TextIcon = GetTypeIcon( type ),
			Category = GetTypeCategory( type )
		};
	}

	public static string GetTypeTitle( Type type )
	{
		var titleAttribute = type.GetCustomAttribute<TitleAttribute>();

		string str = "";
		if ( titleAttribute != null )
			str = titleAttribute.title;

		if ( string.IsNullOrEmpty( str ) )
			str = type.ToString();

		return str;
	}

	public static string GetTypeIcon( Type type )
	{
		var iconAttribute = type.GetCustomAttribute<IconAttribute>();

		if ( iconAttribute != null )
			return iconAttribute.icon;

		return "";
	}

	public static string GetTypeCategory( Type type )
	{
		var categoryAttribute = type.GetCustomAttribute<CategoryAttribute>();

		if ( categoryAttribute != null )
			return categoryAttribute.Category;

		return "";
	}
}
