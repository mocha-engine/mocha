namespace Mocha.AssetCompiler;

[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public class HandlesAttribute : Attribute
{
	public string[] Extensions;

	public HandlesAttribute( string[] extensions )
	{
		Extensions = extensions;
	}
}
