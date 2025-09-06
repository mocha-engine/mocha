namespace Mocha.Common;

[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public class TitleAttribute : Attribute
{
	public string title;

	public TitleAttribute( string title )
	{
		this.title = title;
	}
}
