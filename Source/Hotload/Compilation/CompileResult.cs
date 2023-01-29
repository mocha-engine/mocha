namespace Mocha.Hotload;

/// <summary>
/// Represents a final compilation result.
/// </summary>
internal readonly struct CompileResult
{
	/// <summary>
	/// Whether or not the compilation completed successfully.
	/// </summary>
	internal bool WasSuccessful { get; }

	/// <summary>
	/// The bytes of the compiled assembly.
	/// </summary>
	internal byte[]? CompiledAssembly { get; }
	/// <summary>
	/// The bytes of the symbols in the assembly.
	/// </summary>
	internal byte[]? CompiledAssemblySymbols { get; }
	/// <summary>
	/// An array of errors that occurred during compilation.
	/// </summary>
	internal string[]? Errors { get; }

	/// <summary>
	/// Whether or not the compilation has assembly symbols.
	/// </summary>
	internal bool HasSymbols => CompiledAssemblySymbols is not null;

	private CompileResult( bool wasSuccessful, byte[]? compiledAssembly = null, byte[]? compiledAssemblySymbols = null, string[]? errors = null )
	{
		WasSuccessful = wasSuccessful;

		CompiledAssembly = compiledAssembly;
		CompiledAssemblySymbols = compiledAssemblySymbols;
		Errors = errors;
	}

	/// <summary>
	/// Shorthand method to create a failed <see cref="CompileResult"/>.
	/// </summary>
	/// <param name="errors">An array containing all of the errors that happened during the compilation.</param>
	/// <returns>The newly created <see cref="CompileResult"/>.</returns>
	internal static CompileResult Failed( string[] errors )
	{
		return new CompileResult(
			wasSuccessful: false,
			errors: errors
		);
	}

	/// <summary>
	/// Shorthand method to create a successful <see cref="CompileResult"/>.
	/// </summary>
	/// <param name="compiledAssembly">The bytes of the compiled assembly.</param>
	/// <param name="compiledAssemblySymbols">The bytes of the symbols contained in the compiled assembly.</param>
	/// <returns>The newly created <see cref="CompileResult"/>.</returns>
	internal static CompileResult Successful( byte[] compiledAssembly, byte[]? compiledAssemblySymbols )
	{
		return new CompileResult(
			wasSuccessful: true,
			compiledAssembly: compiledAssembly,
			compiledAssemblySymbols: compiledAssemblySymbols
		);
	}
}
