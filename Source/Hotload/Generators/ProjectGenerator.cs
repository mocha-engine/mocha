namespace Mocha.Hotload;

/// <summary>
/// Generates a .csproj based on a given ProjectManfiest
/// </summary>
internal static class ProjectGenerator
{
	/*
	* - We want to generate a csproj based on the manifest given
	* 
	* - 1. Generate the project file's contents:
	*      + Basic properties (output type, target framework, etc)
	*      + References to all the assemblies in the project
	*      + How do we specify files to compile?
	* - 2. Save the project file to disk
	* - 3. Use the path of the project file for compilation - should
	*      probably return this in a variable somewhere
	*/

	/// <summary>
	/// Generates a csproj for the given project and returns the path to the
	/// generated project on disk.
	/// </summary>
	/// <returns>An absolute path to the generated .csproj file.</returns>
	internal static string GenerateProject( ProjectManifest manifest )
	{
		// Generate the project file's contents
		var baseReferenceDir = Path.GetFullPath( "build\\" );
		var generatedProject = new GeneratedProject( baseReferenceDir, manifest );
		var generatedProjectContents = generatedProject.TransformText();

		// Write the project file to disk
		var destinationPath = Path.Combine( manifest.Resources.Code, "code.csproj" );
		File.WriteAllText( destinationPath, generatedProjectContents );

		// Return the destination path
		return destinationPath;
	}
}
