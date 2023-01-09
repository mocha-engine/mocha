namespace Mocha.AssetCompiler;

/// <summary>
/// Marks a <see cref="BaseCompiler"/> to handle specified file extensions.
/// </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public class HandlesAttribute : Attribute
{
	/// <summary>
	/// The file extensions the compiler can handle.
	/// </summary>
	public string[] Extensions { get; }
	
	public HandlesAttribute( params string[] extensions )
	{
		Extensions = extensions;
	}
}
