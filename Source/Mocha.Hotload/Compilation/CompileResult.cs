using Microsoft.CodeAnalysis;

namespace Mocha.Hotload.Compilation;

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
	/// The workspace that can be used for incremental builds.
	/// </summary>
	internal AdhocWorkspace? Workspace { get; }

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

	/// <summary>
	/// Initializes a new instance of <see cref="CompileResult"/>.
	/// </summary>
	/// <param name="wasSuccessful">Whether or not the compilation was successful.</param>
	/// <param name="workspace">The workspace that was created/updated. Null if <see ref="wasSuccessful"/> is false.</param>
	/// <param name="compiledAssembly">The compiled assembly in a byte array. Null if <see ref="wasSuccessful"/> is false.</param>
	/// <param name="compiledAssemblySymbols">The compiled assembly's debug symbols. Null if no symbols or if <see ref="wasSuccessful"/> is false.</param>
	/// <param name="errors">An array containing all errors that occurred during compilation. Null if <see ref="wasSuccessful"/> is true.</param>
	private CompileResult( bool wasSuccessful, AdhocWorkspace? workspace, byte[]? compiledAssembly = null, byte[]? compiledAssemblySymbols = null, string[]? errors = null )
	{
		WasSuccessful = wasSuccessful;

		Workspace = workspace;
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
			workspace: null,
			errors: errors
		);
	}

	/// <summary>
	/// Shorthand method to create a successful <see cref="CompileResult"/>.
	/// </summary>
	/// <param name="compiledAssembly">The bytes of the compiled assembly.</param>
	/// <param name="compiledAssemblySymbols">The bytes of the symbols contained in the compiled assembly. Null if no debug symbols.</param>
	/// <returns>The newly created <see cref="CompileResult"/>.</returns>
	internal static CompileResult Successful( AdhocWorkspace workspace, byte[] compiledAssembly, byte[]? compiledAssemblySymbols )
	{
		return new CompileResult(
			wasSuccessful: true,
			workspace: workspace,
			compiledAssembly: compiledAssembly,
			compiledAssemblySymbols: compiledAssemblySymbols
		);
	}
}
