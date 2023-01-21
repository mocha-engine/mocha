using System.Text.Json;

namespace Mocha;

partial struct ProjectManifest
{
	public static ProjectManifest Load( string path )
	{
		if ( !File.Exists( path ) )
		{
			throw new Exception( $"Failed to load project at path '{path}'" );
		}

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

		resources.Code = Path.GetFullPath( resources.Code, baseDir );
		resources.Content = Path.GetFullPath( resources.Content, baseDir );
		projectManifest.Resources = resources;

		return projectManifest;
	}
}
