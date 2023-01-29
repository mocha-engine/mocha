namespace Mocha.Hotload.Compilation;

public struct CompileResult
{
	public readonly bool WasSuccessful;

	public readonly byte[]? CompiledAssembly;
	public readonly byte[]? CompiledAssemblySymbols;
	public readonly string[]? Errors;

	public bool HasSymbols => CompiledAssemblySymbols is not null;

	private CompileResult( bool wasSuccessful, byte[]? compiledAssembly = null, byte[]? compiledAssemblySymbols = null, string[]? errors = null )
	{
		WasSuccessful = wasSuccessful;

		CompiledAssembly = compiledAssembly;
		CompiledAssemblySymbols = compiledAssemblySymbols;
		Errors = errors;
	}

	public static CompileResult Failed( string[] errors )
	{
		return new CompileResult(
			wasSuccessful: false,
			errors: errors
		);
	}

	public static CompileResult Successful( byte[] compiledAssembly, byte[]? compiledAssemblySymbols )
	{
		return new CompileResult(
			wasSuccessful: true,
			compiledAssembly: compiledAssembly,
			compiledAssemblySymbols: compiledAssemblySymbols
		);
	}
}
