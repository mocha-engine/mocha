using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Mocha.Common;

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

public class Compiler
{
	private static string[] s_SystemReferences = new[]
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

	private static string s_Globals = """
		global using static Mocha.Common.Global;
		""";

	private static Compiler s_Instance;
	public static Compiler Instance
	{
		get
		{
			s_Instance ??= new();
			return s_Instance;
		}
	}

	private List<PortableExecutableReference> CreateMetadataReferencesFromPaths( string[] assemblyPaths )
	{
		var references = new List<PortableExecutableReference>();

		foreach ( var path in assemblyPaths )
		{
			references.Add( MetadataReference.CreateFromFile( path ) );
		}

		return references;
	}

	public CompileResult Compile( ProjectAssemblyInfo assemblyInfo, CompileOptions? compileOptions = null )
	{
		using var _ = new Stopwatch( $"{assemblyInfo.AssemblyName} compile" );

		if ( compileOptions is null )
		{
			compileOptions = new CompileOptions
			{
				OptimizationLevel = OptimizationLevel.Debug,
				GenerateSymbols = true,
			};
		}

		//
		// Fetch the project and all source files
		//
		var project = new Microsoft.Build.Evaluation.Project( assemblyInfo.ProjectPath );

		var syntaxTrees = new List<SyntaxTree>();
		var embeddedTexts = new List<EmbeddedText>();

		// Global namespaces, etc.
		syntaxTrees.Add( CSharpSyntaxTree.ParseText( s_Globals ) );

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
			{
				var embeddedText = EmbeddedText.FromSource( filePath, sourceText );
				
				embeddedTexts.Add( embeddedText );
			}
		}

		//
		// Build up references
		//
		var references = new List<PortableExecutableReference>();

		// System references
		string dotnetBaseDir = Path.GetDirectoryName( typeof( object ).Assembly.Location );
		foreach ( var systemReference in s_SystemReferences )
		{
			references.Add( MetadataReference.CreateFromFile( Path.Combine( dotnetBaseDir, systemReference ) ) );
		}

		// Mocha references
		references.AddRange( CreateMetadataReferencesFromPaths( new[]
		{
			// TODO: Ideally shouldn't be hardcoding the paths for these here
			"build\\Mocha.Engine.dll",
			"build\\Mocha.Common.dll",
			"build\\Mocha.UI.dll",
			"build\\VConsoleLib.dll",

			// Add a reference to ImGUI too (this is for the editor project -- we should probably
			// allow users to configure custom imports somewhere!)
			"build\\ImGui.NET.dll",
		} ) );

		//
		// Set up compiler
		//

		var options = new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary )
			.WithAllowUnsafe( true )
			.WithPlatform( Platform.X64 )
			.WithOptimizationLevel( compileOptions.OptimizationLevel )
			.WithConcurrentBuild( true );

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
}
