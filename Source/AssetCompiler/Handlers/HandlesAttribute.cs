namespace Mocha.AssetCompiler;

[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public class HandlesAttribute : Attribute
{
	public string[] Extensions { get; }
	
	public HandlesAttribute( params string[] extensions )
	{
		Extensions = extensions;
	}
}
