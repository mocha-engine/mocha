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
	/// An exception that represents the reason for the compile to fail.
	/// </summary>
	public Exception? Exception { get; init; }

	/// <summary>
	/// The resulting data from the compilation.
	/// </summary>
	public ReadOnlyMemory<byte> Data { get; init; }

	/// <summary>
	/// The resulting data of any other associated portions of the compilation.
	/// </summary>
	public IReadOnlyDictionary<string, ReadOnlyMemory<byte>> AssociatedData { get; init; }
}
