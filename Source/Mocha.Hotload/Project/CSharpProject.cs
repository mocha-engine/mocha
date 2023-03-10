using Mocha.Hotload.Compilation;
using Mocha.Hotload.Util;
using NuGet.Versioning;
using System.Collections.Immutable;
using System.Xml;

namespace Mocha.Hotload.Projects;

/// <summary>
/// A representation of a C# project file (.csproj).
/// </summary>
internal sealed class CSharpProject : IEquatable<CSharpProject>
{
	/// <summary>
	/// A cache containing <see cref="CSharpProject"/>s that have been loaded from a file.
	/// </summary>
	private static readonly Dictionary<string, CSharpProject> s_fileCache = new();

	/// <summary>
	/// Type of output to generate (WinExe, Exe, or Library).
	/// </summary>
	internal string OutputType { get; } = "Library";
	/// <summary>
	/// Framework that this project targets. Must be a Target Framework Moniker (e.g. netcoreapp1.0).
	/// </summary>
	internal string TargetFramework { get; } = CompilerHelper.GetCSharpProjectMoniker();
	/// <summary>
	/// Whether or not to enable implicit global usings for the C# project.
	/// </summary>
	internal bool ImplicitUsings { get; } = false;
	/// <summary>
	/// Whether or not to enable nullable annotations and warnings context for the C# project.
	/// </summary>
	internal bool Nullable { get; } = false;
	/// <summary>
	/// Whether or not to allow unsafe code in the project.
	/// </summary>
	internal bool AllowUnsafeBlocks { get; } = false;
	/// <summary>
	/// The version of the C# language to use.
	/// </summary>
	internal string LangVersion { get; } = "latest";
	/// <summary>
	/// An array of all platforms that the C# project supports.
	/// </summary>
	internal ImmutableArray<string> Platforms { get; } = ImmutableArray<string>.Empty;
	/// <summary>
	/// The name of the output C# assembly.
	/// </summary>
	internal string AssemblyName { get; } = "MochaAssembly";
	/// <summary>
	/// The default namespace of the C# project.
	/// </summary>
	internal string RootNamespace { get; } = "Mocha";
	/// <summary>
	/// All of the pre-processor symbols defined in the project.
	/// </summary>
	internal ImmutableArray<string> PreProcessorSymbols { get; } = ImmutableArray<string>.Empty;

	/// <summary>
	/// A set of entries for global using statements.
	/// </summary>
	internal ImmutableDictionary<string, bool> Usings { get; } = ImmutableDictionary<string, bool>.Empty;
	/// <summary>
	/// An array of all the C# files that are in the project.
	/// </summary>
	internal ImmutableArray<string> CSharpFiles { get; } = ImmutableArray<string>.Empty;
	/// <summary>
	/// A set of entries for NuGet packages.
	/// </summary>
	internal ImmutableDictionary<string, NuGetVersion> PackageReferences { get; } = ImmutableDictionary<string, NuGetVersion>.Empty;
	/// <summary>
	/// An array of all the C# project references that this project depends on.
	/// </summary>
	internal ImmutableArray<string> ProjectReferences { get; } = ImmutableArray<string>.Empty;
	/// <summary>
	/// An array of all the DLLs that this project explicitly includes.
	/// </summary>
	internal ImmutableArray<string> DllReferences { get; } = ImmutableArray<string>.Empty;

	/// <summary>
	/// An array of pre-processor symbols defined in a <see cref="ProjectManifest"/>.
	/// </summary>
	private ImmutableArray<string> ProjectPreProcessorSymbols { get; set; } = ImmutableArray<string>.Empty;
	/// <summary>
	/// Contains Xml meta data from <see cref="PackageReferences"/>.
	/// </summary>
	private readonly ImmutableDictionary<string, (string?, string?)> packageReferenceMetaData = ImmutableDictionary<string, (string?, string?)>.Empty;
	/// <summary>
	/// Contains Xml meta data from <see cref="ProjectReferences"/>.
	/// </summary>
	private readonly ImmutableDictionary<string, (string?, string?, bool?)> projectReferenceMetaData = ImmutableDictionary<string, (string?, string?, bool?)>.Empty;

	/// <summary>
	/// Initializes a new instance of <see cref="CSharpProject"/>. This instance is constructed from a csproj file on disk.
	/// </summary>
	/// <param name="filePath">The file path to the csproj file.</param>
	private CSharpProject( string filePath )
	{
		// Setup.
		using var reader = XmlReader.Create( filePath );

		var usings = ImmutableDictionary.CreateBuilder<string, bool>();
		var cSharpFiles = ImmutableArray.CreateBuilder<string>();
		var packageReferences = ImmutableDictionary.CreateBuilder<string, NuGetVersion>();
		var projectReferences = ImmutableArray.CreateBuilder<string>();
		var dllReferences = ImmutableArray.CreateBuilder<string>();

		var packageReferenceMetaDataBuilder = ImmutableDictionary.CreateBuilder<string, (string?, string?)>();
		var projectReferenceMetaDataBuilder = ImmutableDictionary.CreateBuilder<string, (string?, string?, bool?)>();

		var currentMetaDataEntry = string.Empty;
		var isMetaDataPackage = false;

		var shouldTraverse = true;

		// Walk over every Xml element.
		while ( reader.Read() )
		{
			if ( reader.NodeType != XmlNodeType.Element )
				continue;

			if ( !shouldTraverse && reader.Name != "ItemGroup" && reader.Name != "PropertyGroup" )
				continue;

			// Process specific element types.
			switch ( reader.Name )
			{
				case "ItemGroup":
				case "PropertyGroup":
					var condition = reader.GetAttribute( "Condition" );
					if ( condition is null )
					{
						shouldTraverse = true;
						break;
					}

					shouldTraverse = AnalyzeCondition( condition );
					break;
				case "OutputType":
					OutputType = reader.ReadElementContentAsString();
					break;
				case "TargetFramework":
					TargetFramework = reader.ReadElementContentAsString();
					break;
				case "ImplicitUsings":
					ImplicitUsings = reader.ReadElementContentAsString() == "enable";
					break;
				case "AllowUnsafeBlocks":
					AllowUnsafeBlocks = reader.ReadElementContentAsString() == "True";
					break;
				case "LangVersion":
					LangVersion = reader.ReadElementContentAsString();
					break;
				case "Platforms":
					var platforms = ImmutableArray.CreateBuilder<string>();
					platforms.AddRange( reader.ReadElementContentAsString().Split( ';' ) );
					Platforms = platforms.ToImmutableArray();
					break;
				case "Nullable":
					Nullable = reader.ReadElementContentAsString() == "enable";
					break;
				case "AssemblyName":
					AssemblyName = reader.ReadElementContentAsString()
						.Replace( "$(MSBuildProjectName)", Path.GetFileNameWithoutExtension( filePath ) );
					break;
				case "RootNamespace":
					RootNamespace = reader.ReadElementContentAsString()
						.Replace( "$(MSBuildProjectName)", Path.GetFileNameWithoutExtension( filePath ) );
					break;
				case "Using":
					usings.Add( reader.GetAttribute( "Include" )!, reader.GetAttribute( "Static" ) == "true" );
					break;
				// Package references can have extra meta data so setup for that.
				case "PackageReference":
					var packageName = reader.GetAttribute( "Include" )!;
					currentMetaDataEntry = packageName;
					isMetaDataPackage = true;

					packageReferences.Add( packageName, new NuGetVersion( reader.GetAttribute( "Version" ) ) );
					break;
				// Project references can have extra meta data so setup for that.
				case "ProjectReference":
					var projectPath = reader.GetAttribute( "Include" )!;
					currentMetaDataEntry = projectPath;
					isMetaDataPackage = false;

					projectReferences.Add( projectPath );
					break;
				case "Reference":
					dllReferences.Add( reader.GetAttribute( "Include" )! );
					break;
				case "DefineConstants":
					var preProcessorSymbols = ImmutableArray.CreateBuilder<string>();
					preProcessorSymbols.AddRange( reader.ReadElementContentAsString().Split( ';' ) );
					PreProcessorSymbols = preProcessorSymbols.ToImmutableArray();
					break;
				case "IncludeAssets":
					var includeAssets = reader.ReadElementContentAsString();
					if ( isMetaDataPackage )
					{
						if ( !packageReferenceMetaDataBuilder.TryAdd( currentMetaDataEntry, (includeAssets, null) ) )
							packageReferenceMetaDataBuilder[currentMetaDataEntry] = (includeAssets,
								packageReferenceMetaDataBuilder[currentMetaDataEntry].Item2);
					}
					else
					{
						if ( !projectReferenceMetaDataBuilder.TryAdd( currentMetaDataEntry, (includeAssets, null, null) ) )
							projectReferenceMetaDataBuilder[currentMetaDataEntry] = (includeAssets,
								projectReferenceMetaDataBuilder[currentMetaDataEntry].Item2,
								projectReferenceMetaDataBuilder[currentMetaDataEntry].Item3);
					}
					break;
				case "PrivateAssets":
					var privateAssets = reader.ReadElementContentAsString();
					if ( isMetaDataPackage )
					{
						if ( !packageReferenceMetaDataBuilder.TryAdd( currentMetaDataEntry, (null, privateAssets) ) )
							packageReferenceMetaDataBuilder[currentMetaDataEntry] = (packageReferenceMetaDataBuilder[currentMetaDataEntry].Item1,
								privateAssets);
					}
					else
					{
						if ( !projectReferenceMetaDataBuilder.TryAdd( currentMetaDataEntry, (null, privateAssets, null) ) )
							projectReferenceMetaDataBuilder[currentMetaDataEntry] = (projectReferenceMetaDataBuilder[currentMetaDataEntry].Item1,
								privateAssets,
								projectReferenceMetaDataBuilder[currentMetaDataEntry].Item3);
					}
					break;
				case "ReferenceOutputAssembly":
					var referenceOutputAssembly = reader.ReadElementContentAsBoolean();

					if ( !projectReferenceMetaDataBuilder.TryAdd( currentMetaDataEntry, (null, null, referenceOutputAssembly) ) )
						projectReferenceMetaDataBuilder[currentMetaDataEntry] = (projectReferenceMetaDataBuilder[currentMetaDataEntry].Item1,
							projectReferenceMetaDataBuilder[currentMetaDataEntry].Item2,
							referenceOutputAssembly);
					break;
			}
		}

		// Get all C# files in the project.
		foreach ( var file in Directory.EnumerateFiles( Path.GetDirectoryName( filePath )!, "*.cs", SearchOption.AllDirectories ) )
		{
			// TODO: Filter out any directories that may have cs files we don't want.
			if ( file.Contains( "\\obj\\" ) )
			{
				if ( !ImplicitUsings || Path.GetFileName( file ) != "code.GlobalUsings.g.cs" )
					continue;
			}

			cSharpFiles.Add( file );
		}

		// Assign.
		Usings = usings.ToImmutable();
		CSharpFiles = cSharpFiles.ToImmutable();
		PackageReferences = packageReferences.ToImmutable();
		ProjectReferences = projectReferences.ToImmutable();
		DllReferences = dllReferences.ToImmutable();

		// Add to cache.
		if ( s_fileCache.ContainsKey( filePath ) )
			s_fileCache[filePath] = this;
		else
			s_fileCache.Add( filePath, this );
	}

	/// <summary>
	/// Initializes a new instance of <see cref="CSharpProject"/>. This instance is constructed from a <see cref="ProjectManifest"/>.
	/// </summary>
	/// <param name="manifest">The manifest to use for construction.</param>
	private CSharpProject( in ProjectManifest manifest )
	{
		var project = manifest.Project;

		// Basic configuration.
		ImplicitUsings = project.ImplicitUsings;
		AllowUnsafeBlocks = project.AllowUnsafeBlocks;
		Platforms = ImmutableArray.Create( Environment.Is64BitProcess ? "x64" : "x86" );
		Nullable = project.Nullable;
		AssemblyName = manifest.Name;
		RootNamespace = project.DefaultNamespace ?? "Mocha";
		if ( project.PreProcessorSymbols is not null )
			ProjectPreProcessorSymbols = project.PreProcessorSymbols.ToImmutableArray();

		// Usings.
		{
			var usingsBuilder = ImmutableDictionary.CreateBuilder<string, bool>();

			if ( project.UseMochaGlobal ?? true )
				usingsBuilder.Add( "Mocha.Common.Global", true );

			if ( project.Usings is not null )
			{
				foreach ( var usingEntry in project.Usings )
					usingsBuilder.Add( usingEntry.Namespace, usingEntry.Static );
			}

			Usings = usingsBuilder.ToImmutable();
		}

		// Package references.
		{
			var packageReferencesBuilder = ImmutableDictionary.CreateBuilder<string, NuGetVersion>();
			var packageReferenceMetaDataBuilder = ImmutableDictionary.CreateBuilder<string, (string?, string?)>();

			if ( project.PackageReferences is not null )
			{
				foreach ( var packageReference in project.PackageReferences )
				{
					packageReferencesBuilder.Add( packageReference.Name, new NuGetVersion( packageReference.Version ) );
					packageReferenceMetaDataBuilder.Add( packageReference.Name, (packageReference.IncludeAssets, packageReference.PrivateAssets) );
				}
			}

			if ( packageReferencesBuilder.Count > 0 )
			{
				PackageReferences = packageReferencesBuilder.ToImmutable();
				packageReferenceMetaData = packageReferenceMetaDataBuilder.ToImmutable();
			}
		}

		// Project references.
		{
			var projectReferencesBuilder = ImmutableArray.CreateBuilder<string>();
			var projectReferenceMetaDataBuilder = ImmutableDictionary.CreateBuilder<string, (string?, string?, bool?)>();

			if ( project.ProjectReferences is not null )
			{
				foreach ( var projectReference in project.ProjectReferences )
				{
					projectReferencesBuilder.Add( projectReference.Path );
					projectReferenceMetaDataBuilder.Add( projectReference.Path,
						(projectReference.OutputItemType,
						projectReference.PrivateAssets,
						projectReference.ReferenceOutputAssembly) );
				}
			}

			if ( projectReferencesBuilder.Count > 0 )
			{
				ProjectReferences = projectReferencesBuilder.ToImmutable();
				projectReferenceMetaData = projectReferenceMetaDataBuilder.ToImmutable();
			}
		}

		// DLL references.
		{
			var dllReferencesBuilder = ImmutableArray.CreateBuilder<string>();

			if ( project.References is not null )
			{
				foreach ( var reference in project.References )
					dllReferencesBuilder.Add( reference );
			}

			if ( dllReferencesBuilder.Count > 0 )
				DllReferences = dllReferencesBuilder.ToImmutable();
		}
	}

	/// <summary>
	/// Converts the <see cref="CSharpProject"/> to its Xml representation.
	/// </summary>
	/// <returns>The <see cref="XmlDocument"/> that represents the <see cref="CSharpProject"/>.</returns>
	internal XmlDocument ToXml()
	{
		// Setup.
		var baseReferenceDir = Path.GetFullPath( "build\\" );
		var document = new XmlDocument();
		var rootElement = document.CreateElement( "Project" );
		rootElement.SetAttribute( "Sdk", "Microsoft.NET.Sdk" );
		document.AppendChild( rootElement );

		// Basic configuration.
		{
			var basics = rootElement.CreateElement( "PropertyGroup" );
			basics.CreateElementWithInnerText( "OutputType", OutputType );
			basics.CreateElementWithInnerText( "TargetFramework", TargetFramework );
			basics.CreateElementWithInnerText( "ImplicitUsings", ImplicitUsings ? "enable" : "disable" );
			basics.CreateElementWithInnerText( "AllowUnsafeBlocks", AllowUnsafeBlocks ? "True" : "False" );
			basics.CreateElementWithInnerText( "LangVersion", LangVersion );
			basics.CreateElementWithInnerText( "Platforms", string.Join( ';', Platforms ) );
			basics.CreateElementWithInnerText( "Nullable", Nullable ? "enable" : "disable" );
			basics.CreateElementWithInnerText( "AssemblyName", AssemblyName );
			basics.CreateElementWithInnerText( "RootNamespace", RootNamespace );
			basics.CreateElementWithInnerText( "Configurations", "DebugClient;DebugServer;ReleaseClient;ReleaseServer" );
		}

		// Constant definitions.
		{
			const string debugDefinitions = "DEBUG;TRACE;";
			const string clientDefinitions = "MOCHA;CLIENT;";
			const string serverDefinitions = "MOCHA;SERVER;";
			var customDefinitions = string.Join( ';', ProjectPreProcessorSymbols );


			// Mocha client.
			rootElement.CreateElementWithAttributes( "PropertyGroup", "Condition", "'$(Configuration)'=='DebugClient'" )
				.CreateElementWithInnerText( "DefineConstants", (debugDefinitions + clientDefinitions + customDefinitions).Trim( ';' ) );

			rootElement.CreateElementWithAttributes( "PropertyGroup", "Condition", "'$(Configuration)'=='ReleaseClient'" )
				.CreateElementWithInnerText( "DefineConstants", (clientDefinitions + customDefinitions).Trim( ';' ) );

			// Mocha dedicated server.
			rootElement.CreateElementWithAttributes( "PropertyGroup", "Condition", "'$(Configuration)'=='DebugServer'" )
				.CreateElementWithInnerText( "DefineConstants", (debugDefinitions + serverDefinitions + customDefinitions).Trim( ';' ) );

			rootElement.CreateElementWithAttributes( "PropertyGroup", "Condition", "'$(Configuration)'=='ReleaseServer'" )
				.CreateElementWithInnerText( "DefineConstants", (serverDefinitions + customDefinitions).Trim( ';' ) );
		}

		// Implicit usings.
		if ( Usings.Count > 0 )
		{
			var usings = rootElement.CreateElement( "ItemGroup" );
			foreach ( var usingDef in Usings )
				usings.CreateElementWithAttributes( "Using", "Include", usingDef.Key, "Static", usingDef.Value ? "true" : "false" );
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
		if ( PackageReferences.Count > 0 )
		{
			var packageReferences = rootElement.CreateElement( "ItemGroup" );
			foreach ( var packageReference in PackageReferences )
			{
				var element = packageReferences.CreateElementWithAttributes( "PackageReference",
					"Include", packageReference.Key,
					"Version", packageReference.Value.Version.ToString() );

				var metaData = packageReferenceMetaData[packageReference.Key];
				if ( metaData.Item1 is not null )
					element.CreateElementWithInnerText( "IncludeAssets", metaData.Item1 );
				if ( metaData.Item2 is not null )
					element.CreateElementWithInnerText( "PrivateAssets", metaData.Item2 );
			}
		}

		// Project references.
		if ( ProjectReferences.Length > 0 )
		{
			var projectReferences = rootElement.CreateElement( "ItemGroup" );
			foreach ( var projectReference in ProjectReferences )
			{
				var element = projectReferences.CreateElementWithAttributes( "ProjectReference", "Include", baseReferenceDir + projectReference );

				var metaData = projectReferenceMetaData[projectReference];
				if ( metaData.Item1 is not null )
					element.CreateElementWithInnerText( "IncludeAssets", metaData.Item1 );
				if ( metaData.Item2 is not null )
					element.CreateElementWithInnerText( "PrivateAssets", metaData.Item2 );
				if ( metaData.Item3 is not null )
					element.CreateElementWithInnerText( "ReferenceOutputAssembly", metaData.Item3.Value ? "true" : "false" );
			}
		}

		// Literal DLL references.
		if ( DllReferences.Length > 0 )
		{
			var references = rootElement.CreateElement( "ItemGroup" );
			foreach ( var reference in DllReferences )
				references.CreateElementWithAttributes( "Reference", "Include", baseReferenceDir + reference );
		}

		return document;
	}

	/// <inheritdoc/>
	public override bool Equals( object? obj ) => Equals( obj as CSharpProject );

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return HashCode.Combine(
			OutputType,
			TargetFramework,
			ImplicitUsings,
			Nullable,
			AllowUnsafeBlocks,
			LangVersion,
			Platforms,
			AssemblyName )
			+
		HashCode.Combine(
			RootNamespace,
			Usings,
			CSharpFiles,
			PackageReferences,
			ProjectReferences,
			DllReferences,
			packageReferenceMetaData,
			projectReferenceMetaData );
	}

	/// <inheritdoc/>
	public bool Equals( CSharpProject? other )
	{
		if ( other is null )
			return false;

		if ( ReferenceEquals( this, other ) )
			return true;

		// Check basic configuration.
		if ( OutputType != other.OutputType ||
			TargetFramework != other.TargetFramework ||
			ImplicitUsings != other.ImplicitUsings ||
			Nullable != other.Nullable ||
			AllowUnsafeBlocks != other.AllowUnsafeBlocks ||
			LangVersion != other.LangVersion ||
			!Platforms.SequenceEqual( other.Platforms ) ||
			AssemblyName != other.AssemblyName ||
			RootNamespace != other.RootNamespace )
			return false;

		// Check usings.
		if ( Usings.Count != other.Usings.Count )
			return false;

		foreach ( var usingEntry in Usings )
		{
			if ( !other.Usings.TryGetValue( usingEntry.Key, out var value ) )
				return false;

			if ( usingEntry.Value != value )
				return false;
		}

		// Check NuGet package references.
		if ( PackageReferences.Count != other.PackageReferences.Count )
			return false;

		foreach ( var packageReference in PackageReferences )
		{
			if ( !other.PackageReferences.TryGetValue( packageReference.Key, out var value ) )
				return false;

			if ( packageReference.Value != value )
				return false;
		}

		// Check project references.
		if ( ProjectReferences.Length != other.ProjectReferences.Length )
			return false;

		foreach ( var projectReference in ProjectReferences )
		{
			if ( !other.ProjectReferences.Contains( projectReference ) )
				return false;
		}

		// Check DLL references.
		if ( DllReferences.Length != other.DllReferences.Length )
			return false;

		foreach ( var dllReference in DllReferences )
		{
			if ( !other.DllReferences.Contains( dllReference ) )
				return false;
		}

		// Check NuGet package reference meta data.
		if ( packageReferenceMetaData.Count != other.packageReferenceMetaData.Count )
			return false;

		foreach ( var packageReferenceMeta in packageReferenceMetaData )
		{
			if ( !other.packageReferenceMetaData.TryGetValue( packageReferenceMeta.Key, out var tuple ) )
				return false;

			if ( packageReferenceMeta.Value.Item1 != tuple.Item1 ||
				packageReferenceMeta.Value.Item2 != tuple.Item2 )
				return false;
		}

		// Check project reference meta data.
		if ( projectReferenceMetaData.Count != other.projectReferenceMetaData.Count )
			return false;

		foreach ( var projectReferenceMeta in projectReferenceMetaData )
		{
			if ( !other.projectReferenceMetaData.TryGetValue( projectReferenceMeta.Key, out var tuple ) )
				return false;

			if ( projectReferenceMeta.Value.Item1 != tuple.Item1 ||
				projectReferenceMeta.Value.Item2 != tuple.Item2 ||
				projectReferenceMeta.Value.Item3 != tuple.Item3 )
				return false;
		}

		return true;
	}

	/// <summary>
	/// Returns whether or not the condition provided is true.
	/// </summary>
	/// <param name="condition">The condition to parse.</param>
	/// <returns>Whether or not the condition provided is true.</returns>
	private static bool AnalyzeCondition( string condition )
	{
		// TODO: Do we need to flesh this out more?
		condition = condition.Replace( "$(Configuration)", CompilerHelper.Build + CompilerHelper.Realm ).Replace( "'", "" );
		var operands = condition.Split( "==" );

		return operands[0] == operands[1];
	}

	/// <summary>
	/// Parses a csproj file into a <see cref="CSharpProject"/> to be returned. If the csproj was previously parsed then that cached version will be returned.
	/// </summary>
	/// <param name="filePath">The file path to the csproj file.</param>
	/// <returns>The parsed <see cref="CSharpProject"/>. If previously parsed, the cached version is returned.</returns>
	internal static CSharpProject FromFile( string filePath )
	{
		if ( s_fileCache.TryGetValue( filePath, out var csproj ) )
			return csproj;

		return new( filePath );
	}

	/// <summary>
	/// Parses a <see cref="ProjectManifest"/> into a <see cref="CSharpProject"/>.
	/// </summary>
	/// <param name="manifest">The <see cref="ProjectManifest"/> to construct a <see cref="CSharpProject"/> from.</param>
	/// <returns>The parsed <see cref="CSharpProject"/>.</returns>
	internal static CSharpProject FromManifest( in ProjectManifest manifest )
	{
		return new( manifest );
	}

	/// <summary>
	/// Removes a previously cached <see cref="CSharpProject"/>.
	/// </summary>
	/// <param name="filePath">The file path that was used to cache the <see cref="CSharpProject"/>.</param>
	/// <returns>Whether or not an entry was removed.</returns>
	internal static bool RemoveCachedProject( string filePath )
	{
		return s_fileCache.Remove( filePath );
	}

	/// <summary>
	/// Clears the <see cref="CSharpProject"/> file cache.
	/// </summary>
	internal static void ClearCachedProjects()
	{
		s_fileCache.Clear();
	}

	public static bool operator ==( CSharpProject first, CSharpProject second ) => first.Equals( second );
	public static bool operator !=( CSharpProject first, CSharpProject second ) => !first.Equals( second );
}
