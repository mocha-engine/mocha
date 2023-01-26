using System.Text.Json.Serialization;

namespace Mocha.Hotload;

public partial struct ProjectManifest
{
	[JsonPropertyName( "name" )]
	public string Name { get; set; }

	[JsonPropertyName( "author" )]
	public string Author { get; set; }

	[JsonPropertyName( "version" )]
	public string Version { get; set; }

	[JsonPropertyName( "description" )]
	public string Description { get; set; }

	public struct ResourceInfo
	{
		[JsonPropertyName( "code" )]
		public string Code { get; set; }

		[JsonPropertyName( "content" )]
		public string Content { get; set; }
	}

	[JsonPropertyName( "resources" )]
	public ResourceInfo Resources { get; set; }

	public struct PropertyInfo
	{
		[JsonPropertyName( "tickRate" )]
		public int TickRate { get; set; }
	}

	[JsonPropertyName( "properties" )]
	public PropertyInfo Properties { get; set; }

	public struct ProjectInfo
	{
		[JsonPropertyName( "languageVersion" )]
		public string? LanguageVersion { get; set; }

		[JsonPropertyName( "defaultNamespace" )]
		public string? DefaultNamespace { get; set; }

		[JsonPropertyName( "nullable" )]
		public bool Nullable { get; set; }

		[JsonPropertyName( "implicitUsings" )]
		public bool ImplicitUsings { get; set; }

		[JsonPropertyName( "allowUnsafeBlocks" )]
		public bool AllowUnsafeBlocks { get; set; }

		[JsonPropertyName( "useMochaGlobal" )]
		public bool? UseMochaGlobal { get; set; }

		[JsonPropertyName( "usings" )]
		public Using[]? Usings { get; set; }

		[JsonPropertyName( "packageReferences" )]
		public PackageReference[]? PackageReferences { get; set; }

		[JsonPropertyName( "projectReferences" )]
		public ProjectReference[]? ProjectReferences { get; set; }

		[JsonPropertyName( "rawEntry" )]
		public string? RawEntry { get; set; }

		public struct Using
		{
			[JsonPropertyName( "namespace" )]
			public string Namespace { get; set; }

			[JsonPropertyName( "static" )]
			public bool Static { get; set; }
		}

		public struct PackageReference
		{
			[JsonPropertyName( "name" )]
			public string Name { get; set; }

			[JsonPropertyName( "version" )]
			public string Version { get; set; }

			[JsonPropertyName( "privateAssets" )]
			public string? PrivateAssets { get; set; }

			[JsonPropertyName( "includeAssets" )]
			public string? IncludeAssets { get; set; }
		}

		public struct ProjectReference
		{
			[JsonPropertyName( "path" )]
			public string Path { get; set; }

			[JsonPropertyName( "privateAssets" )]
			public string? PrivateAssets { get; set; }

			[JsonPropertyName( "referenceOutputAssembly" )]
			public bool? ReferenceOutputAssembly { get; set; }

			[JsonPropertyName( "outputItemType" )]
			public string? OutputItemType { get; set; }
		}
	}

	[JsonPropertyName( "project" )]
	public ProjectInfo Project { get; set; }
}
