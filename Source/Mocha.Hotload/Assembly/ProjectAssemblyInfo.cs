﻿namespace Mocha.Hotload;

/// <summary>
/// Represents the required information for a project assembly.
/// </summary>
internal readonly struct ProjectAssemblyInfo
{
	/// <summary>
	/// The name of the projects assembly.
	/// </summary>
	internal string AssemblyName { get; init; }

	/// <summary>
	/// The relative path to the project.
	/// </summary>
	internal string SourceRoot { get; init; }

	/// <summary>
	/// The relative path to the projects csproj file.
	/// </summary>
	internal string ProjectPath { get; init; }

	/// <summary>
	/// Are we building this assembly for the server?
	/// If not, we can safely assume that it's for the client.
	/// </summary>
	internal bool IsServer { get; init; }
}
