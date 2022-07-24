namespace Mocha.Engine;

[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
internal class TitleAttribute : Attribute
{
	public string title;

	public TitleAttribute( string title )
	{
		this.title = title;
	}
}
