namespace Mocha.AssetCompiler;

public abstract class BaseCompiler
{
	/// <summary>
	/// The name of the asset this compiler works with.
	/// </summary>
	public abstract string AssetName { get; }

	/// <summary>
	/// The file extension attributed to the asset this compiler works with.
	/// </summary>
	public abstract string CompiledExtension { get; }

	/// <summary>
	/// Whether or not the asset uses <see cref="MochaFile{T}"/> for its (de)serialization.
	/// </summary>
	public abstract bool SupportsMochaFile { get; }

	/// <summary>
	/// An array of file patterns that can be associated with the assets compilation.
	/// </summary>
	public virtual string[] AssociatedFiles => Array.Empty<string>();

	/// <summary>
	/// Compiles the asset.
	/// </summary>
	/// <param name="input"></param>
	/// <returns>The result of the compilation.</returns>
	public abstract CompileResult Compile( ref CompileInput input );

	/// <summary>
	/// Returns a <see cref="CompileState.Succeeded"/> result with the provided data.
	/// </summary>
	/// <param name="data">The compiled data.</param>
	/// <param name="associatedData">The compiled version of any associated data.</param>
	/// <returns>The created result of the compilation.</returns>
	protected static CompileResult Succeeded( ReadOnlyMemory<byte> data, IReadOnlyDictionary<string, ReadOnlyMemory<byte>>? associatedData = null )
	{
		return new CompileResult()
		{
			State = CompileState.Succeeded,
			Exception = null,
			Data = data,
			AssociatedData = associatedData ?? new Dictionary<string, ReadOnlyMemory<byte>>( 0 )
		};
	}

	/// <summary>
	/// Returns a <see cref="CompileState.Failed"/> result with the provided <see cref="Exception"/>.
	/// </summary>
	/// <param name="exception">The exception that occurred during the compilation.</param>
	/// <returns>The created result of the compilation.</returns>
	protected static CompileResult Failed( Exception? exception = null )
	{
		return new CompileResult()
		{
			State = CompileState.Failed,
			Exception = exception
		};
	}
}
