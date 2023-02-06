using System.Text;
using System.Xml;

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
	internal static string Generate( in ProjectManifest manifest )
	{
		// Setup.
		var destinationPath = Path.Combine( manifest.Resources.Code, "code.csproj" );
		var baseReferenceDir = Path.GetFullPath( "build\\" );
		var project = manifest.Project;

		var document = new XmlDocument();
		var rootElement = document.CreateElement( "Project" );
		rootElement.SetAttribute( "Sdk", "Microsoft.NET.Sdk" );
		document.AppendChild( rootElement );

		// Basic configuration.
		{
			var basics = rootElement.CreateElement( "PropertyGroup" );
			basics.CreateElementWithInnerText( "OutputType", "Library" );
			basics.CreateElementWithInnerText( "TargetFramework", "net7.0" );
			basics.CreateElementWithInnerText( "ImplicitUsings", project.ImplicitUsings ? "enable" : "disable" );
			basics.CreateElementWithInnerText( "AllowUnsafeBlocks", project.AllowUnsafeBlocks ? "True" : "False" );
			basics.CreateElementWithInnerText( "LangVersion", project.LanguageVersion ?? "latest" );
			basics.CreateElementWithInnerText( "Platforms", "x64" );
			basics.CreateElementWithInnerText( "BaseOutputPath", "$(SolutionDir)..\\build" );
			basics.CreateElementWithInnerText( "OutputPath", "$(SolutionDir)..\\build" );
			basics.CreateElementWithInnerText( "AppendTargetFrameworkToOutputPath", "false" );
			basics.CreateElementWithInnerText( "PreserveCompilationReferences", "true" );
			basics.CreateElementWithInnerText( "PreserveCompilationContext", "true" );
			basics.CreateElementWithInnerText( "Nullable", project.Nullable ? "true" : "false" );
			basics.CreateElementWithInnerText( "AssemblyName", manifest.Name );
			basics.CreateElementWithInnerText( "RootNamespace", project.DefaultNamespace ?? "Mocha" );
		}

		// Implicit usings.
		{
			var usings = rootElement.CreateElement( "ItemGroup" );
			if ( project.UseMochaGlobal ?? true )
				usings.CreateElementWithAttributes( "Using", "Include", "Mocha.Common.Global", "Static", "true" );

			// Add any custom usings.
			if ( project.Usings is not null )
			{
				foreach ( var usingDef in project.Usings )
					usings.CreateElementWithAttributes( "Using", "Include", usingDef.Namespace, "Static", usingDef.Static ? "true" : "false" );
			}

			// Remove the element if there were no usings.
			if ( usings.ChildNodes.Count == 0 )
				rootElement.RemoveChild( usings );
		}

		// Cleanup entries.
		{
			var cleanup = rootElement.CreateElement( "ItemGroup" );
			cleanup.CreateElementWithAttributes( "Compile", "Remove", "bin\\**" );
			cleanup.CreateElementWithAttributes( "EmbeddedResource", "Remove", "bin\\**" );
			cleanup.CreateElementWithAttributes( "None", "Remove", "bin\\**" );
		}

		// Mocha references.
		{
			var references = rootElement.CreateElement( "ItemGroup" );
			references.CreateElementWithAttributes( "Reference", "Include", baseReferenceDir + "Mocha.Common.dll" );
			references.CreateElementWithAttributes( "Reference", "Include", baseReferenceDir + "Mocha.Engine.dll" );
			references.CreateElementWithAttributes( "Reference", "Include", baseReferenceDir + "Mocha.UI.dll" );
		}

		// NuGet package references.
		if ( project.PackageReferences is not null )
		{
			var packageReferences = rootElement.CreateElement( "ItemGroup" );
			foreach ( var packageReference in project.PackageReferences )
			{
				var element = packageReferences.CreateElementWithAttributes( "PackageReference", "Include", packageReference.Name, "Version", packageReference.Version );
				if ( packageReference.IncludeAssets is not null )
					element.CreateElementWithInnerText( "IncludeAssets", packageReference.IncludeAssets );
				if ( packageReference.PrivateAssets is not null )
					element.CreateElementWithInnerText( "PrivateAssets", packageReference.PrivateAssets );
			}
		}

		// Project references.
		if ( project.ProjectReferences is not null )
		{
			var projectReferences = rootElement.CreateElement( "ItemGroup" );
			foreach ( var projectReference in project.ProjectReferences )
			{
				var element = projectReferences.CreateElementWithAttributes( "PackageReference", "Include", baseReferenceDir + projectReference.Path );
				if ( projectReference.OutputItemType is not null )
					element.CreateElementWithInnerText( "IncludeAssets", projectReference.OutputItemType );
				if ( projectReference.PrivateAssets is not null )
					element.CreateElementWithInnerText( "PrivateAssets", projectReference.PrivateAssets );
				if ( projectReference.ReferenceOutputAssembly is not null )
					element.CreateElementWithInnerText( "PrivateAssets", projectReference.ReferenceOutputAssembly.Value ? "true" : "false" );
			}
		}

		// Literal DLL references.
		if ( project.References is not null )
		{
			var references = rootElement.CreateElement( "ItemGroup" );
			foreach ( var reference in project.References )
				references.CreateElementWithAttributes( "Reference", "Include", baseReferenceDir + reference );
		}

		// Write csproj to disk.
		var stream = File.OpenWrite( destinationPath );
		var writer = new XmlTextWriter( stream, Encoding.UTF8 )
		{
			Formatting = Formatting.Indented,
		};
		document.WriteContentTo( writer );
		writer.Flush();
		writer.Close();

		// Return the destination path.
		return destinationPath;
	}
}
