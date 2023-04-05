using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Mocha.Hotload.Projects;
using System.Text;

namespace Mocha.Hotload.Compilation;

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

		"System.Net.Http.dll",
		"System.Web.HttpUtility.dll",

		"System.Xml.ReaderWriter.dll",
	};

	private static readonly Dictionary<string, PortableExecutableReference> s_referenceCache = new();

	/// <summary>
	/// Compiles a given project assembly.
	/// </summary>
	/// <param name="assemblyInfo">The project assembly to compile.</param>
	/// <param name="compileOptions">The options to give to the C# compilation.</param>
	/// <returns>A <see cref="Task"/> that represents the asynchronous operation. The <see cref="Task"/>s return value is the result of the compilation.</returns>
	/// <exception cref="System.Diagnostics.UnreachableException">Thrown when the final workspace project becomes invalid unexpectedly.</exception>
	internal static async Task<CompileResult> CompileAsync( ProjectAssemblyInfo assemblyInfo, CompileOptions? compileOptions = null )
	{
		compileOptions ??= new CompileOptions
		{
			OptimizationLevel = OptimizationLevel.Debug,
			GenerateSymbols = true
		};

		// Setup a basic list of tasks.
		var basicTasks = new List<Task>();

		//
		// Fetch the project and all source files.
		//
		var csproj = CSharpProject.FromFile( assemblyInfo.ProjectPath );
		var parseOptions = CSharpParseOptions.Default
			.WithPreprocessorSymbols( csproj.PreProcessorSymbols );

		var syntaxTrees = new List<SyntaxTree>();

		// Build syntax trees.
		{
			// Global namespaces.
			var globalUsings = string.Empty;
			foreach ( var usingEntry in csproj.Usings )
				globalUsings += $"global using{(usingEntry.Value ? " static " : " ")}{usingEntry.Key};{Environment.NewLine}";

			if ( globalUsings != string.Empty )
				syntaxTrees.Add( CSharpSyntaxTree.ParseText( globalUsings, options: parseOptions, encoding: Encoding.UTF8 ) );

			// For each source file, create a syntax tree we can use to compile it.
			foreach ( var filePath in csproj.CSharpFiles )
			{
				// Add the parsed syntax tree.
				basicTasks.Add( Task.Run( async () =>
				{
					var text = await File.ReadAllTextAsync( filePath );
					syntaxTrees.Add( CSharpSyntaxTree.ParseText( text, options: parseOptions, encoding: Encoding.UTF8, path: filePath ) );
				} ) );
			}

			// Wait for all tasks to finish before continuing.
			await Task.WhenAll( basicTasks );
			// Clear this list for any users later on.
			basicTasks.Clear();
		}

		// Stripping syntax trees.
		{
			// Strip out methods marked with [ServerOnly] or [ClientOnly] attribute based on the current realm.
			var newSyntaxTrees = new List<SyntaxTree>();
			// Which attribute do we want to remove (or, in other words, which realm are we not in).
			var targetAttribute = assemblyInfo.IsServer ? "ClientOnly" : "ServerOnly";
			var stripTasks = new List<Task<SyntaxTree?>>();

			// Walk all syntax trees and strip them.
			foreach ( var tree in syntaxTrees )
				stripTasks.Add( StripSyntaxTreeAsync( tree, targetAttribute ) );

			// Wait for all tasks to finish before continuing.
			await Task.WhenAll( stripTasks );

			// Add all stripped syntax trees.
			foreach ( var stripTask in stripTasks )
			{
				if ( stripTask.Result is null )
					continue;

				newSyntaxTrees.Add( stripTask.Result );
			}

			syntaxTrees = newSyntaxTrees;
		}

		//
		// Build up references.
		//
		var references = new List<PortableExecutableReference>();
		{
			// System references.
			var dotnetBaseDir = Path.GetDirectoryName( typeof( object ).Assembly.Location )!;
			foreach ( var systemReference in s_systemReferences )
				references.Add( CreateMetadataReferenceFromPath( Path.Combine( dotnetBaseDir, systemReference ) ) );

			// NuGet references.
			foreach ( var packageReference in csproj.PackageReferences )
				basicTasks.Add( NuGetHelper.FetchPackageAsync( packageReference.Key, packageReference.Value, references ) );

			// Wait for all tasks to finish before continuing.
			await Task.WhenAll( basicTasks );
			// Clear this list for any users later on.
			basicTasks.Clear();

			// Project references.
			// TODO: This is nightmare fuel, need a better solution long-term.
			foreach ( var projectReference in csproj.ProjectReferences )
			{
				var referenceCsprojPath = Path.GetFullPath( Path.Combine( Path.GetDirectoryName( assemblyInfo.ProjectPath )!, projectReference ) );
				var referenceProject = CSharpProject.FromFile( referenceCsprojPath );
				var assemblyName = referenceProject.AssemblyName;

				if ( !string.IsNullOrEmpty( assemblyName ) )
					references.Add( CreateMetadataReferenceFromPath( "build\\" + assemblyName + ".dll" ) );
				else
					references.Add( CreateMetadataReferenceFromPath( "build\\" + Path.GetFileNameWithoutExtension( referenceCsprojPath ) + ".dll" ) );
			}

			// Literal references.
			foreach ( var reference in csproj.DllReferences )
				references.Add( CreateMetadataReferenceFromPath( Path.GetFullPath( reference ) ) );
		}

		//
		// Setup compilation.
		//

		// Setup compile options.
		var options = new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary )
			.WithPlatform( Platform.X64 )
			.WithOptimizationLevel( compileOptions.OptimizationLevel )
			.WithConcurrentBuild( true )
			.WithAllowUnsafe( csproj.AllowUnsafeBlocks );

		// Setup incremental workspace.
		var workspace = new AdhocWorkspace();

		// Setup project.
		var projectInfo = Microsoft.CodeAnalysis.ProjectInfo.Create(
			ProjectId.CreateNewId( assemblyInfo.AssemblyName ),
			VersionStamp.Create(),
			assemblyInfo.AssemblyName,
			assemblyInfo.AssemblyName,
			LanguageNames.CSharp,
			compilationOptions: options,
			parseOptions: parseOptions,
			metadataReferences: references );
		var project = workspace.AddProject( projectInfo );

		// Add documents to workspace.
		foreach ( var syntaxTree in syntaxTrees )
		{
			var documentInfo = DocumentInfo.Create(
				DocumentId.CreateNewId( project.Id ),
				Path.GetFileName( syntaxTree.FilePath ),
				filePath: syntaxTree.FilePath,
				sourceCodeKind: SourceCodeKind.Regular,
				loader: TextLoader.From( TextAndVersion.Create( syntaxTree.GetText(), VersionStamp.Create() ) ) );

			workspace.AddDocument( documentInfo );
		}

		project = workspace.CurrentSolution.GetProject( project.Id );

		// Panic if project became invalid.
		if ( project is null )
			throw new System.Diagnostics.UnreachableException();

		//
		// Compile.
		//

		using var assemblyStream = new MemoryStream();
		using var symbolsStream = compileOptions.GenerateSymbols ? new MemoryStream() : null;

		// Setup emit options.
		EmitOptions? emitOptions = null;
		if ( compileOptions.GenerateSymbols )
		{
			emitOptions = new EmitOptions(
				debugInformationFormat: DebugInformationFormat.PortablePdb,
				pdbFilePath: $"{assemblyInfo.AssemblyName}.pdb" );
		}

		// Compile. Panic if compilation becomes invalid.
		var compilation = await project.GetCompilationAsync() ?? throw new System.Diagnostics.UnreachableException();
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

		return CompileResult.Successful( workspace, assemblyStream.ToArray(), symbolsStream?.ToArray() );
	}

	/// <summary>
	/// Compiles a <see cref="Workspace"/> assembly with incremental changes.
	/// </summary>
	/// <param name="workspace">The <see cref="Workspace"/> that contains the code.</param>
	/// <param name="changedFilePaths">A dictionary of absolute file paths mapped to the type of change it has experienced.</param>
	/// <param name="assemblyInfo">The projects <see cref="ProjectAssemblyInfo"/>.</param>
	/// <param name="compileOptions">The <see cref="CompileOptions"/> to give to the C# compilation.</param>
	/// <returns>A <see cref="Task"/> that represents the asynchronous operation. The <see cref="Task"/>s return value is the result of the compilation.</returns>
	/// <exception cref="System.Diagnostics.UnreachableException">Thrown when applying changes to the <see cref="Workspace"/> failed.</exception>
	internal static async Task<CompileResult> IncrementalCompileAsync( AdhocWorkspace workspace, IReadOnlyDictionary<string, WatcherChangeTypes> changedFilePaths, ProjectAssemblyInfo assemblyInfo, CompileOptions? compileOptions = null )
	{
		compileOptions ??= new CompileOptions
		{
			OptimizationLevel = OptimizationLevel.Debug,
			GenerateSymbols = true,
		};
		var parseOptions = (CSharpParseOptions)workspace.CurrentSolution.Projects.First().ParseOptions!;

		// Which attribute do we want to remove (or, in other words, which realm are we not in).
		var targetAttribute = assemblyInfo.IsServer ? "ClientOnly" : "ServerOnly";

		// Update each changed file.
		foreach ( var (filePath, changeType) in changedFilePaths )
		{
			switch ( changeType )
			{
				case WatcherChangeTypes.Created:
					{
						var syntaxTree = CSharpSyntaxTree.ParseText(
							await File.ReadAllTextAsync( filePath ),
							options: parseOptions,
							encoding: Encoding.UTF8,
							path: filePath );
						syntaxTree = await StripSyntaxTreeAsync( syntaxTree, targetAttribute );

						if ( syntaxTree is null )
							continue;

						var documentInfo = DocumentInfo.Create(
							DocumentId.CreateNewId( workspace.CurrentSolution.ProjectIds[0] ),
							Path.GetFileName( syntaxTree.FilePath ),
							filePath: syntaxTree.FilePath,
							sourceCodeKind: SourceCodeKind.Regular,
							loader: TextLoader.From( TextAndVersion.Create( syntaxTree.GetText(), VersionStamp.Create() ) ) );

						workspace.AddDocument( documentInfo );
						if ( !workspace.TryApplyChanges( workspace.CurrentSolution ) )
							throw new System.Diagnostics.UnreachableException();
						break;
					}
				case WatcherChangeTypes.Deleted:
					{
						// Find the existing document for the deleted file.
						var document = workspace.CurrentSolution.GetDocumentIdsWithFilePath( filePath )
							.Select( workspace.CurrentSolution.GetDocument )
							.FirstOrDefault();

						if ( document is null )
							continue;

						// Apply the removed document.
						if ( !workspace.TryApplyChanges( workspace.CurrentSolution.RemoveDocument( document.Id ) ) )
							throw new System.Diagnostics.UnreachableException();
						break;
					}
				case WatcherChangeTypes.Changed:
				case WatcherChangeTypes.Renamed:
					{
						// Find the existing document for the changed file.
						var document = workspace.CurrentSolution.GetDocumentIdsWithFilePath( filePath )
							.Select( workspace.CurrentSolution.GetDocument )
							.FirstOrDefault();

						if ( document is null )
							continue;

						var syntaxTree = CSharpSyntaxTree.ParseText(
							await File.ReadAllTextAsync( filePath ),
							options: parseOptions,
							encoding: Encoding.UTF8,
							path: filePath );
						syntaxTree = await StripSyntaxTreeAsync( syntaxTree, targetAttribute );

						// TODO: Remove the document?
						if ( syntaxTree is null )
							continue;

						// Apply the changed tree.
						if ( !workspace.TryApplyChanges( workspace.CurrentSolution.WithDocumentSyntaxRoot( document.Id, syntaxTree.GetRoot() ) ) )
							throw new System.Diagnostics.UnreachableException();

						break;
					}
			}
		}

		using var assemblyStream = new MemoryStream();
		using var symbolsStream = compileOptions.GenerateSymbols ? new MemoryStream() : null;

		// Setup emit options.
		EmitOptions? emitOptions = null;
		if ( compileOptions.GenerateSymbols )
		{
			emitOptions = new EmitOptions(
				debugInformationFormat: DebugInformationFormat.PortablePdb,
				pdbFilePath: $"{assemblyInfo.AssemblyName}.pdb" );
		}

		// Compile.
		var compilation = await workspace.CurrentSolution.Projects.First().GetCompilationAsync() ?? throw new System.Diagnostics.UnreachableException();
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

		return CompileResult.Successful( workspace, assemblyStream.ToArray(), symbolsStream?.ToArray() );
	}

	/// <summary>
	/// Walks a <see cref="SyntaxTree"/> and strips any <see cref="MemberDeclarationSyntax"/> that use specified attributes.
	/// </summary>
	/// <param name="syntaxTree">The <see cref="SyntaxTree"/> to walk.</param>
	/// <param name="attributesToStrip">The names of all the attributes to search for and strip.</param>
	/// <returns>
	/// A <see cref="Task"/> that represents the asynchronous operation. The <see cref="Task"/>s return value is the stripped <see cref="SyntaxTree"/>.
	/// A null result occurs when the <see cref="SyntaxTree"/> is completely stripped.
	/// </returns>
	private async static Task<SyntaxTree?> StripSyntaxTreeAsync( SyntaxTree syntaxTree, params string[] attributesToStrip )
	{
		var root = await syntaxTree.GetRootAsync();
		var syntaxToStrip = new List<SyntaxNode>();

		// Walk all delcarations and mark them for stripping.
		foreach ( var declaration in root.DescendantNodes().OfType<MemberDeclarationSyntax>() )
		{
			// Ignore namespace declarations.
			if ( declaration is BaseNamespaceDeclarationSyntax )
				continue;

			foreach ( var attributeName in attributesToStrip )
			{
				var attribute = declaration.AttributeLists
				.SelectMany( x => x.Attributes )
				.FirstOrDefault( x => x.Name.ToString() == attributeName );

				if ( attribute is null )
					continue;

				syntaxToStrip.Add( declaration );
				break;
			}
		}

		// Strip all syntax and return the final tree.
		return root.RemoveNodes( syntaxToStrip, SyntaxRemoveOptions.KeepNoTrivia )?.SyntaxTree;
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
		if ( s_referenceCache.TryGetValue( assemblyPath, out var reference ) )
			return reference;

		var newReference = MetadataReference.CreateFromFile( assemblyPath );
		s_referenceCache.Add( assemblyPath, newReference );
		return newReference;
	}
}
