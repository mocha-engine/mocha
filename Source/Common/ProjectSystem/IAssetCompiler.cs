namespace Mocha.AssetCompiler;

public interface IAssetCompiler
{
	static IAssetCompiler? Current { get; set; }

	void CompileFile( string path );
	Task CompileFileAsync( string path );
}
