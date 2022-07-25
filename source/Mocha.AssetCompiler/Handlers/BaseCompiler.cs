namespace Mocha.AssetCompiler;

[Handles( new[] { ".png", ".jpg" } )]
public abstract class BaseCompiler
{
	public abstract string CompileFile( string path );
}
