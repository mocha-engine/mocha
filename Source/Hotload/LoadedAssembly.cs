using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mocha.Common;
using System.Reflection;

namespace Mocha;

public struct LoadedAssemblyInfo
{
	public string AssemblyName;
	public string SourceRoot;
	public string ProjectPath;
}

public class LoadedAssemblyType<T>
{
	private T? managedClass;
	private FileSystemWatcher watcher;
	private LoadedAssemblyInfo assemblyInfo;
	private Assembly assembly;

	public LoadedAssemblyType( LoadedAssemblyInfo assemblyInfo )
	{
		this.assemblyInfo = assemblyInfo;

		CompileIntoMemory();
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

	private void CompileIntoMemory()
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
			"build\\Mocha.Common.dll",
			"build\\Mocha.UI.dll",
			"build\\VConsoleLib.dll",

			// Add a reference to ImGUI too (this is for the editor project -- we should probably
			// allow users to configure custom imports somewhere!)
			"build\\ImGui.NET.dll",
		} ) );

		// If this is the editor project, add a reference to Mocha.Engine
		if ( assemblyInfo.AssemblyName == "Mocha.Editor" )
		{
			references.AddRange( CreateMetadataReferencesFromPaths( new[]
			{
				"build\\Mocha.Engine.dll",
			} ) );
		}

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

		//
		// Compile assembly into memory
		//
		using ( var memoryStream = new MemoryStream() )
		{
			var result = compilation.Emit( memoryStream );

			if ( !result.Success )
			{
				Log.Error( $"Failed to compile {assemblyInfo.AssemblyName}!" );

				IEnumerable<Diagnostic> failures = result.Diagnostics.Where( diagnostic =>
					diagnostic.IsWarningAsError ||
					diagnostic.Severity == DiagnosticSeverity.Error );

				foreach ( Diagnostic diagnostic in failures )
				{
					var lineSpan = diagnostic.Location.GetLineSpan();
					Log.Error( $"\n{diagnostic.Id}: {diagnostic.GetMessage()}\n\tat {lineSpan.Path} line {lineSpan.StartLinePosition.Line}" );
				}

				Environment.Exit( -1 );
			}
			else
			{
				Log.Info( $"Compiled {assemblyInfo.AssemblyName} successfully" );

				// Save result as dll file
				var dllPath = Path.Combine( "build\\", $"{assemblyInfo.AssemblyName}.dll" );
				File.WriteAllBytes( dllPath, memoryStream.ToArray() );

				var name = AssemblyName.GetAssemblyName( dllPath );
				assembly = AppDomain.CurrentDomain.Load( name );

				LoadGameInterface();
			}
		}
	}

	private void LoadGameInterface()
	{
		// Find first type that derives from T
		foreach ( var type in assembly.GetTypes() )
		{
			if ( type.GetInterface( typeof( T ).FullName! ) != null )
			{
				managedClass = (T)Activator.CreateInstance( type )!;
				break;
			}
		}

		if ( managedClass == null )
		{
			throw new Exception( "Could not find IGame implementation" );
		}
	}

	private void CreateFileSystemWatcher( string sourcePath )
	{
		watcher = new FileSystemWatcher( sourcePath, "*.*" );
		watcher.Changed += OnFileChanged;
		watcher.EnableRaisingEvents = true;
	}

	private void OnFileChanged( object sender, FileSystemEventArgs e )
	{
		Log.Trace( $"File {e.FullPath} was changed" );
	}

	public T Value => managedClass!;

	public static implicit operator T( LoadedAssemblyType<T> loadedAssemblyType )
	{
		return loadedAssemblyType.managedClass!;
	}
}
