namespace Mocha.Hotload;

/// <summary>
/// Represents the required information for a project assembly.
/// </summary>
public readonly struct ProjectAssemblyInfo
{
	/// <summary>
	/// The name of the projects assembly.
	/// </summary>
	public string AssemblyName { get; init; }

	/// <summary>
	/// The relative path to the project.
	/// </summary>
	public string SourceRoot { get; init; }

	/// <summary>
	/// The relative path to the projects csproj file.
	/// </summary>
	public string ProjectPath { get; init; }
}
