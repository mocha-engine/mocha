using Microsoft.CodeAnalysis;

namespace Mocha.Hotload;

/// <summary>
/// A container for all options to give to the compiler.
/// </summary>
public class CompileOptions
{
	/// <summary>
	/// The level of optimization to be applied to the source code.
	/// </summary>
	public OptimizationLevel OptimizationLevel { get; set; }

	/// <summary>
	/// Whether or not to generate symbols for the resulting assembly.
	/// NOTE: This will do nothing when <see cref="OptimizationLevel"/> is <see cref="OptimizationLevel.Release"/>.
	/// </summary>
	public bool GenerateSymbols { get; set; }
}
