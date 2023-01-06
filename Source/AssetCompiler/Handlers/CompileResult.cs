namespace Mocha.AssetCompiler;

/// <summary>
/// Represents a result from an asset compilation.
/// </summary>
public readonly struct CompileResult
{
	/// <summary>
	/// The resulting state of the compilation.
	/// </summary>
	public CompileState State { get; init; }

	/// <summary>
	/// The source path of the asset.
	/// </summary>
	public string SourcePath { get; init; }
	/// <summary>
	/// The destination path of the compiled asset.
	/// </summary>
	public string? DestinationPath { get; init; }

	/// <summary>
	/// An exception that represents the reason for the compile to fail.
	/// </summary>
	public Exception? Error { get; init; }
}
