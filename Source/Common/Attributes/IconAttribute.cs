namespace Mocha.Common;

[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public class IconAttribute : Attribute
{
	public string icon;

	public IconAttribute( string icon )
	{
		this.icon = icon;
	}
}
