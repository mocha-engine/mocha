namespace Mocha.Common;

public interface IAssetCompiler
{
	/// <summary>
	/// The currently used asset compiler.
	/// </summary>
	static IAssetCompiler? Current { get; set; }

	/// <summary>
	/// Compiles an asset synchronously.
	/// </summary>
	/// <param name="path">The path to the source file.</param>
	void CompileFile( string path );

	/// <summary>
	/// Compiles an asset asynchronously.
	/// </summary>
	/// <param name="path">The path to the source file.</param>
	/// <returns>The task that represents the asynchronous operation.</returns>
	Task CompileFileAsync( string path );
}
