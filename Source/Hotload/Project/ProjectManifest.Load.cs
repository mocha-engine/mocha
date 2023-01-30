using System.Text.Json;

namespace Mocha.Hotload;

partial struct ProjectManifest
{
	/// <summary>
	/// Converts a relative path in the project manifest to an absolute path.
	/// </summary>
	/// <param name="path">The relative path.</param>
	/// <param name="baseDir">The path to the directory that contained the manifest.</param>
	/// <returns>The constructed absolute path.</returns>
	private static string GetAbsolutePath( string path, string baseDir )
	{
		return Path.GetFullPath( Path.Combine( baseDir, path ) );
	}

	/// <summary>
	/// Loads a project manifest.
	/// </summary>
	/// <param name="path">The absolute path to the manifest file.</param>
	/// <returns>The constructed project manfifest.</returns>
	/// <exception cref="FileNotFoundException">Thrown when no file exists at the given path.</exception>
	internal static ProjectManifest Load( string path )
	{
		if ( !File.Exists( path ) )
			throw new FileNotFoundException( $"Failed to load project at path '{path}'" );

		var fileContents = File.ReadAllText( path );
		var projectManifest = JsonSerializer.Deserialize<ProjectManifest>( fileContents );

		//
		// Post-process the data
		//

		// Convert all paths to absolute paths
		// TODO: We could probably just do this recursively for
		// every element in Resources, or we could attach a custom
		// attribute, or we could use a custom converter...
		var resources = projectManifest.Resources;
		var baseDir = Path.GetDirectoryName( path )!;

		resources.Code = GetAbsolutePath( resources.Code, baseDir );
		resources.Content = GetAbsolutePath( resources.Content, baseDir );
		projectManifest.Resources = resources;

		return projectManifest;
	}
}
