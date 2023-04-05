using System.Text.Json.Serialization;

namespace Mocha.Hotload.Projects;

/// <summary>
/// Represents a manifest containing all the required information for a game project.
/// </summary>
internal partial struct ProjectManifest
{
	/// <summary>
	/// The name of the project.
	/// </summary>
	[JsonPropertyName( "name" )]
	public string Name { get; init; }

	/// <summary>
	/// The author of the project.
	/// </summary>
	[JsonPropertyName( "author" )]
	public string Author { get; set; }

	/// <summary>
	/// The version of the project.
	/// </summary>
	[JsonPropertyName( "version" )]
	public string Version { get; set; }

	/// <summary>
	/// A description of the project.
	/// </summary>
	[JsonPropertyName( "description" )]
	public string Description { get; set; }

	/// <summary>
	/// Contains the path information of the project.
	/// </summary>
	[JsonPropertyName( "resources" )]
	public ResourceInfo Resources { get; set; }

	/// <summary>
	/// Contains the game properties of the project.
	/// </summary>
	[JsonPropertyName( "properties" )]
	public Properties Properties { get; set; }

	/// <summary>
	/// Contains the C# project settings.
	/// </summary>
	[JsonPropertyName( "project" )]
	public ProjectInfo Project { get; set; }
}

/// <summary>
/// Represents the path information in a <see cref="ProjectManifest"/>.
/// </summary>
internal struct ResourceInfo
{
	/// <summary>
	/// The relative path to the C# code directory.
	/// </summary>
	[JsonPropertyName( "code" )]
	public string Code { get; set; }

	/// <summary>
	/// The relative path to the asset content directory.
	/// </summary>
	[JsonPropertyName( "content" )]
	public string Content { get; set; }
}

/// <summary>
/// Represents the game properties in a <see cref="ProjectManifest"/>.
/// </summary>
internal struct Properties
{
	/// <summary>
	/// The tick rate the game should run at.
	/// </summary>
	[JsonPropertyName( "tickRate" )]
	public int TickRate { get; set; }
}

/// <summary>
/// Represents the C# project settings in a <see cref="ProjectManifest"/>.
/// </summary>
internal struct ProjectInfo
{
	/// <summary>
	/// The version of the C# language to use.
	/// </summary>
	[JsonPropertyName( "languageVersion" )]
	public string? LanguageVersion { get; set; }

	/// <summary>
	/// The default namespace in the C# project.
	/// </summary>
	[JsonPropertyName( "defaultNamespace" )]
	public string? DefaultNamespace { get; set; }

	/// <summary>
	/// Whether or not to enable nullable annotations.
	/// </summary>
	[JsonPropertyName( "nullable" )]
	public bool Nullable { get; set; }

	/// <summary>
	/// Whether or not to enable implicit global usings.
	/// </summary>
	[JsonPropertyName( "implicitUsings" )]
	public bool ImplicitUsings { get; set; }

	/// <summary>
	/// Whether or not to enable the use of unsafe code blocks.
	/// </summary>
	[JsonPropertyName( "allowUnsafeBlocks" )]
	public bool AllowUnsafeBlocks { get; set; }

	/// <summary>
	/// Whether or not to use the <see cref="Common.Global"/> class globally in the C# project.
	/// </summary>
	[JsonPropertyName( "useMochaGlobal" )]
	public bool? UseMochaGlobal { get; set; }

	/// <summary>
	/// Contains any custom pre-processor symbols to include in the compilation.
	/// </summary>
	[JsonPropertyName( "preProcessorSymbols" )]
	public string[]? PreProcessorSymbols { get; set; }

	/// <summary>
	/// Contains any custom global using entries.
	/// </summary>
	[JsonPropertyName( "usings" )]
	public Using[]? Usings { get; set; }

	/// <summary>
	/// Contains any NuGet package references.
	/// </summary>
	[JsonPropertyName( "packageReferences" )]
	public PackageReference[]? PackageReferences { get; set; }

	/// <summary>
	/// Contains any C# project references.
	/// </summary>
	[JsonPropertyName( "projectReferences" )]
	public ProjectReference[]? ProjectReferences { get; set; }

	/// <summary>
	/// Contains any literal DLL references.
	/// </summary>
	[JsonPropertyName( "references" )]
	public string[]? References { get; set; }

	/// <summary>
	/// Any C# project items to append to the file after serialization.
	/// </summary>
	[JsonPropertyName( "rawEntry" )]
	public string? RawEntry { get; set; }
}

/// <summary>
/// Represents a using statement in a C# project.
/// </summary>
internal struct Using
{
	/// <summary>
	/// The fully qualified namespace (and class if applicable) to use.
	/// </summary>
	[JsonPropertyName( "namespace" )]
	public string Namespace { get; set; }

	/// <summary>
	/// Whether or not to use the namespace statically.
	/// </summary>
	[JsonPropertyName( "static" )]
	public bool Static { get; set; }
}

/// <summary>
/// Represents a NuGet package reference in a C# project.
/// </summary>
internal struct PackageReference
{
	/// <summary>
	/// The name of the NuGet package.
	/// </summary>
	[JsonPropertyName( "name" )]
	public string Name { get; set; }

	/// <summary>
	/// The version of the NuGet package.
	/// </summary>
	[JsonPropertyName( "version" )]
	public string Version { get; set; }

	/// <summary>
	/// Defines the "PrivateAssets" option in the C# project.
	/// </summary>
	[JsonPropertyName( "privateAssets" )]
	public string? PrivateAssets { get; set; }

	/// <summary>
	/// Defines the "IncludeAssets" option in the C# project.
	/// </summary>
	[JsonPropertyName( "includeAssets" )]
	public string? IncludeAssets { get; set; }
}

/// <summary>
/// Represents a C# project reference in a C# project.
/// </summary>
internal struct ProjectReference
{
	/// <summary>
	/// The path to the csproj file.
	/// </summary>
	[JsonPropertyName( "path" )]
	public string Path { get; set; }

	/// <summary>
	/// Defines the "PrivateAssets" option in the C# project.
	/// </summary>
	[JsonPropertyName( "privateAssets" )]
	public string? PrivateAssets { get; set; }

	/// <summary>
	/// Defines the "OutputItemType" option in the C# project.
	/// </summary>
	[JsonPropertyName( "outputItemType" )]
	public string? OutputItemType { get; set; }

	/// <summary>
	/// Defines the "ReferenceOutputAssembly" option in the C# project.
	/// </summary>
	[JsonPropertyName( "referenceOutputAssembly" )]
	public bool? ReferenceOutputAssembly { get; set; }
}
