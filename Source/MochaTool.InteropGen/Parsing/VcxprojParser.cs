using System.Collections.Frozen;
using System.Xml;

namespace MochaTool.InteropGen.Parsing;

/// <summary>
/// Contains functionality for parsing vcxproj files.
/// </summary>
internal static class VcxprojParser
{
	// Note that these paths only work for the windows x64 platforms right now.
	// If we want other platforms and architectures, this will need changing..
	// InteropGen isn't architecture- or platform-specific in any way, so this
	// should not be a big deal.
	private const string ExternalIncludePath = "/rs:Project/rs:PropertyGroup[@Condition=\"'$(Configuration)|$(Platform)'=='Debug|x64'\"]/rs:ExternalIncludePath";
	private const string IncludePath = "/rs:Project/rs:PropertyGroup[@Condition=\"'$(Configuration)|$(Platform)'=='Debug|x64'\"]/rs:IncludePath";

	private static readonly FrozenDictionary<string, string> EnvironmentVariables = new Dictionary<string, string>()
	{
		{
			"VULKAN_SDK",

			Environment.GetEnvironmentVariable( "VULKAN_SDK" ) ??
			Path.Combine( "C:", "VulkanSDK", "1.3.224.1", "Include" )
		},
		{ "ProjectDir", Path.Combine( "..", "Mocha.Host" ) },
		{ "SolutionDir", $"..{Path.DirectorySeparatorChar}" },
		{ "Platform", "x64" },
		{
			"VcpkgRoot",

			Environment.GetEnvironmentVariable( "VCPKG_ROOT" ) ??
			Path.Combine( "C:", "Users", Environment.UserName, "vcpkg" )
		},
		{ "IncludePath", Path.Combine( "..", "Mocha.Host" ) },
		{ "ExternalIncludePath", "" }
	}.ToFrozenDictionary();

	/// <summary>
	/// Parse the include list from a vcxproj file.
	/// </summary>
	/// <remarks>
	/// This currently only supports x64-windows, so any different includes for other platforms
	/// will not be reflected here.
	/// </remarks>
	internal static List<string> ParseIncludes( string path )
	{
		var doc = new XmlDocument();
		doc.Load( path );

		var namespaceManager = new XmlNamespaceManager( doc.NameTable );
		namespaceManager.AddNamespace( "rs", "http://schemas.microsoft.com/developer/msbuild/2003" );

		if ( doc.DocumentElement is null )
			throw new Exception( "Failed to parse root node!" );

		var root = doc.DocumentElement;

		var includes = new List<string>();

		// Select Project -> PropertyGroup -> ExternalIncludePath
		{
			var includeStr = GetNodeContents( root, ExternalIncludePath, namespaceManager );
			includes.AddRange( includeStr.Split( ';', StringSplitOptions.TrimEntries ) );
		}

		// Select Project -> PropertyGroup -> IncludePath and merge it
		{
			var includeStr = GetNodeContents( root, IncludePath, namespaceManager );
			includes.AddRange( includeStr.Split( ';', StringSplitOptions.TrimEntries ) );
		}

		var parsedIncludes = new List<string>();

		// Simple find-and-replace for macros and environment variables
		foreach ( var include in includes )
		{
			var processedInclude = include;

			foreach ( var environmentVariable in EnvironmentVariables )
				processedInclude = processedInclude.Replace( $"$({environmentVariable.Key})", environmentVariable.Value );

			parsedIncludes.Add( processedInclude );
		}

		return parsedIncludes;
	}

	private static string GetNodeContents( XmlNode root, string xpath, XmlNamespaceManager namespaceManager )
	{
		var nodeList = root.SelectNodes( xpath, namespaceManager );
		if ( nodeList?.Count == 0 || nodeList?[0] is null )
			throw new Exception( "Couldn't find IncludePath!" );

		var includeStr = nodeList[0]!.InnerText;

		return includeStr;
	}
}
