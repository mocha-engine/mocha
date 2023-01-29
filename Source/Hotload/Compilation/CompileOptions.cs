using Microsoft.CodeAnalysis;

namespace Mocha.Hotload;

public class CompileOptions
{
	public OptimizationLevel OptimizationLevel { get; set; }

	// Does nothing with a Release optimization level
	public bool GenerateSymbols { get; set; }
}
