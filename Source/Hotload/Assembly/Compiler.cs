using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mocha.Common;
using System.Reflection;

namespace Mocha.Hotload;

public struct CompileResult
{
	public bool WasSuccessful;

	public Assembly CompiledAssembly;
	public string[] Errors;

	public static CompileResult Failed( string[] errors )
	{
		return new CompileResult
		{
			WasSuccessful = false,
			Errors = errors
		};
	}

	public static CompileResult Successful( Assembly assembly )
	{
		return new CompileResult
		{
			WasSuccessful = true,
			CompiledAssembly = assembly
		};
	}
}

public class Compiler
{
	private static Compiler instance;
	public static Compiler Instance
	{
		get
		{
			instance ??= new();
			return instance;
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

	public CompileResult Compile( LoadedAssemblyInfo assemblyInfo )
	{
		using var _ = new Stopwatch( $"{assemblyInfo.AssemblyName} compile" );

		//
		// Fetch the project and all source files
		//
		var project = new Microsoft.Build.Evaluation.Project( assemblyInfo.ProjectPath );
		var syntaxTrees = new List<SyntaxTree>();

		// For each source file, create a syntax tree we can use to compile it
		foreach ( var item in project.GetItems( "Compile" ) )
		{
			var filePath = item.EvaluatedInclude;

			// Get path based on project root
			filePath = Path.Combine( assemblyInfo.SourceRoot, filePath );

			var fileText = File.ReadAllText( filePath );

			// Append global namespace
			fileText = "global using static Mocha.Common.Global;\n" + fileText;

			var syntaxTree = CSharpSyntaxTree.ParseText( fileText, path: filePath );

			syntaxTrees.Add( syntaxTree );
		}

		//
		// Build up references
		//
		var references = new List<PortableExecutableReference>();

		// System references
		var systemReferences = new[]
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

		string dotnetBaseDir = Path.GetDirectoryName( typeof( object ).Assembly.Location );
		foreach ( var systemReference in systemReferences )
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
		using var memoryStream = new MemoryStream();
		var result = compilation.Emit( memoryStream );

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

		// Load from memory
		var assembly = AppDomain.CurrentDomain.Load( memoryStream.ToArray() );
		return CompileResult.Successful( assembly );
	}
}
