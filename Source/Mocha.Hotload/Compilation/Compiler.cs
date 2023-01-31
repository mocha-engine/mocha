using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Mocha.Common;
using NuGet.Versioning;
using System.Reflection;
using System.Runtime.Versioning;

namespace Mocha.Hotload;

/// <summary>
/// Contains the core functionality for compilation of C# assemblies.
/// </summary>
internal static class Compiler
{
	/// <summary>
	/// The .NET references to include in every build.
	/// </summary>
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

	/// <summary>
	/// Compiles a given project assembly.
	/// </summary>
	/// <param name="assemblyInfo">The project assembly to compile.</param>
	/// <param name="compileOptions">The options to give to the C# compilation.</param>
	/// <returns>A task that represents the asynchronous operation. The tasks return value is the result of the compilation.</returns>
	internal static async Task<CompileResult> Compile( ProjectAssemblyInfo assemblyInfo, CompileOptions? compileOptions = null )
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
		var project = Project.FromFile( assemblyInfo.ProjectPath, new Microsoft.Build.Definition.ProjectOptions() );

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

			// File path should be absolute
			filePath = Path.GetFullPath( filePath );

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
			references.Add( CreateMetadataReferenceFromPath( Path.Combine( dotnetBaseDir, systemReference ) ) );

		// NuGet references
		foreach ( var packageReference in project.GetItems( "PackageReference" ) )
			await NuGetHelper.FetchPackage( packageReference.EvaluatedInclude, new NuGetVersion( packageReference.GetMetadataValue( "Version" ) ), references );

		// Project references
		// TODO: This is nightmare fuel, need a better solution long-term.
		foreach ( var projectReference in project.GetItems( "ProjectReference" ) )
		{
			var referenceCsprojPath = Path.GetFullPath( Path.Combine( Path.GetDirectoryName( assemblyInfo.ProjectPath )!, projectReference.EvaluatedInclude ) );
			var referenceProject = Project.FromFile( referenceCsprojPath, new Microsoft.Build.Definition.ProjectOptions() );
			var assemblyName = referenceProject.GetPropertyValue( "AssemblyName" );

			if ( !string.IsNullOrEmpty( assemblyName ) )
				references.Add( CreateMetadataReferenceFromPath( "build\\" + assemblyName + ".dll" ) );
			else
				references.Add( CreateMetadataReferenceFromPath( "build\\" + Path.GetFileNameWithoutExtension( referenceCsprojPath ) + ".dll" ) );
		}

		// Literal references
		foreach ( var reference in project.GetItems( "Reference" ) )
			references.Add( CreateMetadataReferenceFromPath( Path.GetFullPath( reference.EvaluatedInclude ) ) );

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

		var result = compilation.Emit(
			assemblyStream,
			symbolsStream,
			options: emitOptions
		);

		if ( !result.Success )
		{
			Log.Error( $"Failed to compile {assemblyInfo.AssemblyName}!" );

			var failures = result.Diagnostics.Where( diagnostic =>
				diagnostic.IsWarningAsError ||
				diagnostic.Severity == DiagnosticSeverity.Error );

			var errors = failures.Select( diagnostic =>
			{
				var lineSpan = diagnostic.Location.GetLineSpan();
				return $"\n{diagnostic.Id}: {diagnostic.GetMessage()}\n\tat {lineSpan.Path} line {lineSpan.StartLinePosition.Line}";
			} ).ToArray();

			return CompileResult.Failed( errors );
		}

		Log.Info( $"Compiled {assemblyInfo.AssemblyName} successfully" );

		return CompileResult.Successful( assemblyStream.ToArray(), symbolsStream?.ToArray() );
	}

	/// <summary>
	/// Returns a set of <see cref="PortableExecutableReference"/> from a set of relative paths.
	/// </summary>
	/// <param name="assemblyPaths">A set of relative paths to create references from.</param>
	/// <returns>A set of <see cref="PortableExecutableReference"/> from a set of relative paths.</returns>
	internal static IEnumerable<PortableExecutableReference> CreateMetadataReferencesFromPaths( IEnumerable<string> assemblyPaths )
	{
		foreach ( var assemblyPath in assemblyPaths )
			yield return CreateMetadataReferenceFromPath( assemblyPath );
	}

	/// <summary>
	/// Creates a <see cref="PortableExecutableReference"/> from a relative path.
	/// </summary>
	/// <param name="assemblyPath">The relative path to create a reference from.</param>
	/// <returns>A <see cref="PortableExecutableReference"/> from a relative path.</returns>
	internal static PortableExecutableReference CreateMetadataReferenceFromPath( string assemblyPath )
	{
		return MetadataReference.CreateFromFile( assemblyPath );
	}

	/// <summary>
	/// Returns the target framework of the application.
	/// </summary>
	/// <returns>The target framework of the application.</returns>
	internal static string GetTargetFrameworkName()
	{
		// AppContext.TargetFrameworkName will always be null since the starting process is native code.
		// Leave it here anyway in case this changes.
		if ( !string.IsNullOrEmpty( AppContext.TargetFrameworkName ) )
			return AppContext.TargetFrameworkName;

		// Fallback on the TargetFrameworkAttribute of the Hotload assembly
		if ( Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>() is not TargetFrameworkAttribute frameworkAttribute )
			return string.Empty;

		return frameworkAttribute.FrameworkName;
	}
}
