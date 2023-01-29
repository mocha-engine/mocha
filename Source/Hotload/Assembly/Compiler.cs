using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Mocha.Common;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

namespace Mocha.Hotload;

public class CompileOptions
{
	public OptimizationLevel OptimizationLevel { get; set; }

	// Does nothing with a Release optimization level
	public bool GenerateSymbols { get; set; }
}

public struct CompileResult
{
	public readonly bool WasSuccessful;

	public readonly byte[]? CompiledAssembly;
	public readonly byte[]? CompiledAssemblySymbols;
	public readonly string[]? Errors;

	public bool HasSymbols => CompiledAssemblySymbols is not null;

	private CompileResult( bool wasSuccessful, byte[]? compiledAssembly = null, byte[]? compiledAssemblySymbols = null, string[]? errors = null )
	{
		WasSuccessful = wasSuccessful;

		CompiledAssembly = compiledAssembly;
		CompiledAssemblySymbols = compiledAssemblySymbols;
		Errors = errors;
	}

	public static CompileResult Failed( string[] errors )
	{
		return new CompileResult(
			wasSuccessful: false,
			errors: errors
		);
	}

	public static CompileResult Successful( byte[] compiledAssembly, byte[]? compiledAssemblySymbols )
	{
		return new CompileResult(
			wasSuccessful: true,
			compiledAssembly: compiledAssembly,
			compiledAssemblySymbols: compiledAssemblySymbols
		);
	}
}

public static class Compiler
{
	private static readonly string[] s_systemReferences = new[]
	{
		"mscorlib.dll",
		"System.dll",

		"System.Core.dll",

		"System.ComponentModel.Primitives.dll",
		"System.ComponentModel.Annotations.dll",

		"System.Collections.dll",
		"System.Collections.Concurrent.dll",
		"System.Collections.Immutable.dll",
		"System.Collections.Specialized.dll",

		"System.Console.dll",

		"System.Data.dll",
		"System.Diagnostics.Process.dll",

		"System.IO.Compression.dll",
		"System.IO.FileSystem.Watcher.dll",

		"System.Linq.dll",
		"System.Linq.Expressions.dll",

		"System.Numerics.Vectors.dll",

		"System.ObjectModel.dll",

		"System.Private.CoreLib.dll",
		"System.Private.Xml.dll",
		"System.Private.Uri.dll",

		"System.Runtime.Extensions.dll",
		"System.Runtime.dll",

		"System.Text.RegularExpressions.dll",
		"System.Text.Json.dll",

		"System.Security.Cryptography.dll",

		"System.Threading.Channels.dll",

		"System.Web.HttpUtility.dll",

		"System.Xml.ReaderWriter.dll",
	};

	private static readonly string[] s_mochaReferences = new string[]
	{
		// TODO: Ideally shouldn't be hardcoding the paths for these here
		"build\\Mocha.Engine.dll",
		"build\\Mocha.Common.dll",
		"build\\Mocha.UI.dll",
		"build\\VConsoleLib.dll",

		// Add a reference to ImGUI too (this is for the editor project -- we should probably
		// allow users to configure custom imports somewhere!)
		"build\\ImGui.NET.dll",
	};

	public static async Task<CompileResult> Compile( ProjectAssemblyInfo assemblyInfo, CompileOptions? compileOptions = null )
	{
		using var _ = new Stopwatch( $"{assemblyInfo.AssemblyName} compile" );

		compileOptions ??= new CompileOptions
		{
			OptimizationLevel = OptimizationLevel.Debug,
			GenerateSymbols = true,
		};

		//
		// Fetch the project and all source files
		//
		var project = new Project( assemblyInfo.ProjectPath );

		var syntaxTrees = new List<SyntaxTree>();
		var embeddedTexts = new List<EmbeddedText>();

		// Global namespaces, etc.
		var globalUsings = string.Empty;
		foreach ( var usingEntry in project.GetItems( "Using" ) )
		{
			var isStatic = bool.Parse( usingEntry.GetMetadataValue( "Static" ) );
			globalUsings += $"global using{(isStatic ? " static " : " ")}{usingEntry.EvaluatedInclude};{Environment.NewLine}";
		}

		if ( globalUsings != string.Empty )
			syntaxTrees.Add( CSharpSyntaxTree.ParseText( globalUsings ) );

		// For each source file, create a syntax tree we can use to compile it
		foreach ( var item in project.GetItems( "Compile" ) )
		{
			var filePath = item.EvaluatedInclude;

			// Get path based on project root
			filePath = Path.Combine( assemblyInfo.SourceRoot, filePath );

			var encoding = System.Text.Encoding.Default;

			var fileText = File.ReadAllText( filePath );
			var sourceText = SourceText.From( fileText, encoding );

			var syntaxTree = CSharpSyntaxTree.ParseText( sourceText, path: filePath );

			syntaxTrees.Add( syntaxTree );

			if ( compileOptions.GenerateSymbols )
				embeddedTexts.Add( EmbeddedText.FromSource( filePath, sourceText ) );
		}

		//
		// Build up references
		//
		var references = new List<PortableExecutableReference>();

		// System references
		string dotnetBaseDir = Path.GetDirectoryName( typeof( object ).Assembly.Location )!;
		foreach ( var systemReference in s_systemReferences )
			references.Add( MetadataReference.CreateFromFile( Path.Combine( dotnetBaseDir, systemReference ) ) );

		// Mocha references
		references.AddRange( CreateMetadataReferencesFromPaths( s_mochaReferences ) );

		// NuGet references
		foreach ( var packageReference in project.GetItems( "PackageReference" ) )
			await FetchPackage( packageReference.EvaluatedInclude, new NuGetVersion( packageReference.GetMetadataValue( "Version" ) ), references );

		//
		// Set up compiler
		//

		var options = new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary )
			.WithPlatform( Platform.X64 )
			.WithOptimizationLevel( compileOptions.OptimizationLevel )
			.WithConcurrentBuild( true );

		var unsafeBlocksAllowed = project.GetPropertyValue( "AllowUnsafeBlocks" );
		if ( unsafeBlocksAllowed != string.Empty )
			options = options.WithAllowUnsafe( bool.Parse( unsafeBlocksAllowed ) );
		else
			options = options.WithAllowUnsafe( false );

		var compilation = CSharpCompilation.Create(
			assemblyInfo.AssemblyName,
			syntaxTrees,
			references,
			options
		);

		// Unload project
		project.ProjectCollection.UnloadProject( project );

		//
		// Compile assembly into memory
		//
		using var assemblyStream = new MemoryStream();
		using var symbolsStream = compileOptions.GenerateSymbols ? new MemoryStream() : null;

		EmitOptions? emitOptions = null;

		if ( compileOptions.GenerateSymbols )
		{
			emitOptions = new EmitOptions(
						debugInformationFormat: DebugInformationFormat.PortablePdb,
						pdbFilePath: $"{assemblyInfo.AssemblyName}.pdb" );
		}

		EmitResult result = compilation.Emit(
			assemblyStream,
			symbolsStream,
			options: emitOptions
		);

		if ( !result.Success )
		{
			Log.Error( $"Failed to compile {assemblyInfo.AssemblyName}!" );

			IEnumerable<Diagnostic> failures = result.Diagnostics.Where( diagnostic =>
				diagnostic.IsWarningAsError ||
				diagnostic.Severity == DiagnosticSeverity.Error );

			string[] errors = failures.Select( diagnostic =>
			{
				var lineSpan = diagnostic.Location.GetLineSpan();
				return $"\n{diagnostic.Id}: {diagnostic.GetMessage()}\n\tat {lineSpan.Path} line {lineSpan.StartLinePosition.Line}";
			} ).ToArray();

			return CompileResult.Failed( errors );
		}

		Log.Info( $"Compiled {assemblyInfo.AssemblyName} successfully" );

		return CompileResult.Successful( assemblyStream.ToArray(), symbolsStream?.ToArray() );
	}

	private static IEnumerable<PortableExecutableReference> CreateMetadataReferencesFromPaths( string[] assemblyPaths )
	{
		foreach ( var assemblyPath in assemblyPaths )
			yield return CreateMetadataReferenceFromPath( assemblyPath );
	}

	private static PortableExecutableReference CreateMetadataReferenceFromPath( string assemblyPath )
	{
		return MetadataReference.CreateFromFile( assemblyPath );
	}

	private static string GetTargetFrameworkName()
	{
		if ( !string.IsNullOrEmpty( AppContext.TargetFrameworkName ) )
			return AppContext.TargetFrameworkName;

		if ( Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>() is not TargetFrameworkAttribute frameworkAttribute )
			return string.Empty;

		return frameworkAttribute.FrameworkName;
	}

	private static async Task FetchPackage( string id, NuGetVersion version, ICollection<PortableExecutableReference> references )
	{
		var logger = NullLogger.Instance;
		var cancellationToken = CancellationToken.None;

		var cache = new SourceCacheContext();
		var repository = Repository.Factory.GetCoreV3( "https://api.nuget.org/v3/index.json" );
		var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

		using var packageStream = new MemoryStream();

		await resource.CopyNupkgToStreamAsync(
			id,
			version,
			packageStream,
			cache,
			logger,
			cancellationToken );

		using var packageReader = new PackageArchiveReader( packageStream );
		var nuspecReader = await packageReader.GetNuspecReaderAsync( cancellationToken );

		var currentFramework = NuGetFramework.ParseFrameworkName( GetTargetFrameworkName(), DefaultFrameworkNameProvider.Instance );
		var targetFrameworkGroup = NuGetFrameworkExtensions.GetNearest( packageReader.GetLibItems(), currentFramework );
		var dependencies = nuspecReader.GetDependencyGroups().First( group => group.TargetFramework == targetFrameworkGroup.TargetFramework ).Packages.ToArray();

		if ( dependencies.Length > 0 )
		{
			foreach ( var dependency in dependencies )
				await FetchPackageWithVersionRange( dependency.Id, dependency.VersionRange, references );
		}

		if ( !targetFrameworkGroup.Items.Any() )
			return;

		var dllFile = targetFrameworkGroup.Items.FirstOrDefault( item => item.EndsWith( "dll" ) );
		if ( dllFile is null )
			return;

		packageReader.ExtractFile( dllFile, Path.Combine( Directory.GetCurrentDirectory(), $"build\\{id}.dll" ), logger );
		references.Add( CreateMetadataReferenceFromPath( $"build\\{id}.dll" ) );
	}

	private static async Task FetchPackageWithVersionRange( string id, VersionRange versionRange, ICollection<PortableExecutableReference> references )
	{
		var cache = new SourceCacheContext();
		var repository = Repository.Factory.GetCoreV3( "https://api.nuget.org/v3/index.json" );
		var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

		var versions = await resource.GetAllVersionsAsync(
			id,
			cache,
			NullLogger.Instance,
			CancellationToken.None
			);

		var bestVersion = versionRange.FindBestMatch( versions );
		await FetchPackage( id, bestVersion, references );
	}
}
