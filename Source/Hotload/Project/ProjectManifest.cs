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
		[JsonPropertyName( "defaultNamespace" )]
		public string DefaultNamespace { get; set; }

		[JsonPropertyName( "nullable" )]
		public bool Nullable { get; set; }

		[JsonPropertyName( "implicitUsings" )]
		public bool ImplicitUsings { get; set; }
	}

	[JsonPropertyName( "project" )]
	public ProjectInfo Project { get; set; }
}
