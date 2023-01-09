namespace Mocha.AssetCompiler;

/// <summary>
/// Represents an input to a asset compiler.
/// </summary>
public readonly struct CompileInput
{
	/// <summary>
	/// The absolute path to the asset the <see ref="Data"/> came from. Null if in memory.
	/// </summary>
	public string? SourcePath { get; init; }

	/// <summary>
	/// The main data portion for the asset.
	/// </summary>
	public ReadOnlyMemory<byte> SourceData { get; init; }

	/// <summary>
	/// Contains any extra data to be compiled alongside the <see ref="SourceData"/>.
	/// </summary>
	public IReadOnlyDictionary<string, ReadOnlyMemory<byte>> AssociatedData { get; init; }

	/// <summary>
	/// The <see cref="System.Security.Cryptography.MD5"/> hash of all data files.
	/// </summary>
	public byte[] DataHash { get; init; }
}
