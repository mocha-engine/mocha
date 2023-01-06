namespace Mocha.AssetCompiler;

public abstract class BaseCompiler
{
	/// <summary>
	/// The name of the asset this compiler works with.
	/// </summary>
	public abstract string AssetName { get; }

	/// <summary>
	/// Compiles the asset.
	/// </summary>
	/// <param name="path">The path to the source asset.</param>
	/// <returns>The result of the compilation.</returns>
	public abstract CompileResult CompileFile( string path );

	protected static CompileResult UpToDate( string sourcePath, string destinationPath )
	{
		return new CompileResult()
		{
			State = CompileState.UpToDate,
			SourcePath = sourcePath,
			DestinationPath = destinationPath,
			Error = null
		};
	}
	protected static CompileResult Succeeded( string sourcePath, string destinationPath )
	{
		return new CompileResult()
		{
			State = CompileState.Succeeded,
			SourcePath = sourcePath,
			DestinationPath = destinationPath,
			Error = null
		};
	}
	protected static CompileResult Failed( string sourcePath, string? destinationPath = null, Exception? exception = null )
	{
		return new CompileResult()
		{
			State = CompileState.Failed,
			SourcePath = sourcePath,
			DestinationPath = destinationPath,
			Error = exception
		};
	}
}
