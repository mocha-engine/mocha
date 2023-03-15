using Microsoft.Extensions.Logging;

namespace MochaTool.InteropGen;

/// <summary>
/// The main entry point to the IntropGen program.
/// </summary>
public static class Program
{
	/// <summary>
	/// Contains all of the parsed units to generate bindings for.
	/// </summary>
	private static readonly List<IUnit> s_units = new();
	/// <summary>
	/// Contains all of the files that need to be generated.
	/// </summary>
	private static readonly List<string> s_files = new();

	/// <summary>
	/// The entry point to the program.
	/// </summary>
	/// <param name="args">The command-line arguments given to the program.</param>
	public static void Main( string[] args )
	{
		using var _totalTime = new StopwatchLog( "InteropGen", LogLevel.Information );

		var baseDir = args[0];
		Log.LogIntro();

		//
		// Prep
		//
		DeleteExistingFiles( baseDir );

		using ( var _parseTime = new StopwatchLog( "Parsing" ) )
			Parse( baseDir );

		//
		// Expand methods out into list of (method name, method)
		//
		var methods = s_units.OfType<Class>().SelectMany( unit => unit.Methods, ( unit, method ) => (unit.Name, method) ).ToList();

		//
		// Write files
		//
		WriteManagedStruct( baseDir, methods );
		WriteNativeStruct( baseDir, methods );
		WriteNativeIncludes( baseDir );
	}

	/// <summary>
	/// Deletes and re-creates the generated file directories.
	/// </summary>
	/// <param name="baseDir">The base directory that contains the source projects.</param>
	private static void DeleteExistingFiles( string baseDir )
	{
		var destCsDir = $"{baseDir}\\Mocha.Common\\Glue";
		var destHeaderDir = $"{baseDir}\\Mocha.Host\\generated";

		if ( Directory.Exists( destHeaderDir ) )
			Directory.Delete( destHeaderDir, true );
		if ( Directory.Exists( destCsDir ) )
			Directory.Delete( destCsDir, true );

		Directory.CreateDirectory( destHeaderDir );
		Directory.CreateDirectory( destCsDir );
	}

	/// <summary>
	/// Parses all header files in the Mocha.Host project for interop generation.
	/// </summary>
	/// <param name="baseDir">The base directory that contains the source projects.</param>
	private static void Parse( string baseDir )
	{
		// Find and queue all of the header files to parse.
		var queue = new List<string>();
		QueueDirectory( queue, baseDir + "\\Mocha.Host" );

		// Dispatch jobs to parse all files.
		var dispatcher = new ThreadDispatcher<string>( async ( files ) =>
		{
			foreach ( var path in files )
				await ProcessHeaderAsync( baseDir, path );
		}, queue );

		// Wait for all threads to finish...
		while ( !dispatcher.IsComplete )
			Thread.Sleep( 1 );
	}

	/// <summary>
	/// Writes the C# unmanaged arguments.
	/// </summary>
	/// <param name="baseDir">The base directory that contains the source projects.</param>
	/// <param name="methods">An enumerable list of all of the methods to write in the struct.</param>
	private static void WriteManagedStruct( string baseDir, IEnumerable<(string Name, Method method)> methods )
	{
		var (baseManagedStructWriter, managedStructWriter) = Utils.CreateWriter();

		managedStructWriter.WriteLine( "using System.Runtime.InteropServices;" );
		managedStructWriter.WriteLine();
		managedStructWriter.WriteLine( "[StructLayout( LayoutKind.Sequential )]" );
		managedStructWriter.WriteLine( "public struct UnmanagedArgs" );
		managedStructWriter.WriteLine( '{' );

		managedStructWriter.Indent++;

		managedStructWriter.WriteLine( "public IntPtr __Root;" );

		var managedStructBody = string.Join( "\r\n\t", methods.Select( x => $"public IntPtr __{x.Name}_{x.method.Name}MethodPtr;" ) );
		managedStructWriter.Write( managedStructBody );
		managedStructWriter.WriteLine();

		managedStructWriter.Indent--;

		managedStructWriter.WriteLine( '}' );
		managedStructWriter.Dispose();

		File.WriteAllText( $"{baseDir}/Mocha.Common/Glue/UnmanagedArgs.cs", baseManagedStructWriter.ToString() );
	}

	/// <summary>
	/// Writes the C++ unmanaged arguments.
	/// </summary>
	/// <param name="baseDir">The base directory that contains the source projects.</param>
	/// <param name="methods">An enumerable list of all of the methods to write in the struct.</param>
	private static void WriteNativeStruct( string baseDir, IEnumerable<(string Name, Method method)> methods )
	{
		var (baseNativeStructWriter, nativeStructWriter) = Utils.CreateWriter();

		nativeStructWriter.WriteLine( "#ifndef __GENERATED_UNMANAGED_ARGS_H" );
		nativeStructWriter.WriteLine( "#define __GENERATED_UNMANAGED_ARGS_H" );
		nativeStructWriter.WriteLine( "#include \"InteropList.generated.h\"" );
		nativeStructWriter.WriteLine();
		nativeStructWriter.WriteLine( "struct UnmanagedArgs" );
		nativeStructWriter.WriteLine( '{' );
		nativeStructWriter.Indent++;

		nativeStructWriter.WriteLine( "void* __Root;" );

		var nativeStructBody = string.Join( "\r\n\t", methods.Select( x => $"void* __{x.Name}_{x.method.Name}MethodPtr;" ) );
		nativeStructWriter.Write( nativeStructBody );
		nativeStructWriter.WriteLine();

		nativeStructWriter.Indent--;
		nativeStructWriter.WriteLine( "};" );
		nativeStructWriter.WriteLine();

		nativeStructWriter.WriteLine( "inline UnmanagedArgs args" );
		nativeStructWriter.WriteLine( '{' );
		nativeStructWriter.Indent++;

		nativeStructWriter.WriteLine( "Root::GetInstance()," );

		nativeStructBody = string.Join( ",\r\n\t", methods.Select( x => $"(void*)__{x.Name}_{x.method.Name}" ) );
		nativeStructWriter.Write( nativeStructBody );
		nativeStructWriter.WriteLine();

		nativeStructWriter.Indent--;
		nativeStructWriter.WriteLine( "};" );

		nativeStructWriter.WriteLine();
		nativeStructWriter.WriteLine( "#endif // __GENERATED_UNMANAGED_ARGS_H" );
		nativeStructWriter.Dispose();

		File.WriteAllText( $"{baseDir}Mocha.Host\\generated\\UnmanagedArgs.generated.h", baseNativeStructWriter.ToString() );
	}

	/// <summary>
	/// Writes the C++ includes for the host project.
	/// </summary>
	/// <param name="baseDir">The base directory that contains the source projects.</param>
	private static void WriteNativeIncludes( string baseDir )
	{
		var (baseNativeListWriter, nativeListWriter) = Utils.CreateWriter();

		nativeListWriter.WriteLine( "#ifndef __GENERATED_INTEROPLIST_H" );
		nativeListWriter.WriteLine( "#define __GENERATED_INTEROPLIST_H" );
		nativeListWriter.WriteLine();
		nativeListWriter.Indent++;

		var nativeListBody = string.Join( "\r\n\t", s_files.Select( x => $"#include \"{x}.generated.h\"" ) );
		nativeListWriter.Write( nativeListBody );
		nativeListWriter.WriteLine();

		nativeListWriter.Indent--;
		nativeListWriter.WriteLine();
		nativeListWriter.WriteLine( "#endif // __GENERATED_INTEROPLIST_H" );

		File.WriteAllText( $"{baseDir}Mocha.Host\\generated\\InteropList.generated.h", baseNativeListWriter.ToString() );
	}

	/// <summary>
	/// Parses a header file and generates its C# and C++ interop code.
	/// </summary>
	/// <param name="baseDir">The base directory that contains the source projects.</param>
	/// <param name="path"></param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private static async Task ProcessHeaderAsync( string baseDir, string path )
	{
		Log.ProcessingHeader( path );

		// Parse header.
		var units = Parser.GetUnits( path );

		// Generate interop code.
		var managedCode = ManagedCodeGenerator.GenerateCode( units );
		var relativePath = Path.GetRelativePath( $"{baseDir}/Mocha.Host/", path );
		var nativeCode = NativeCodeGenerator.GenerateCode( relativePath, units );

		// Write interop code.
		var fileName = Path.GetFileNameWithoutExtension( path );
		var csTask = File.WriteAllTextAsync( $"{baseDir}Mocha.Common\\Glue\\{fileName}.generated.cs", managedCode );
		var nativeTask = File.WriteAllTextAsync( $"{baseDir}Mocha.Host\\generated\\{fileName}.generated.h", nativeCode );

		// Wait for writing to finish.
		await Task.WhenAll( csTask, nativeTask );

		s_files.Add( fileName );
		s_units.AddRange( units );
	}

	/// <summary>
	/// Searches the directory for any header files that should be parsed.
	/// </summary>
	/// <param name="queue">The queue collection to append to.</param>
	/// <param name="directory">The absolute path to the directory to search for files.</param>
	private static void QueueDirectory( ICollection<string> queue, string directory )
	{
		foreach ( var file in Directory.GetFiles( directory, "*.h" ) )
		{
			if ( file.EndsWith( ".generated.h" ) )
				continue;

			var fileContents = File.ReadAllText( file );
			if ( !fileContents.Contains( "GENERATE_BINDINGS", StringComparison.CurrentCultureIgnoreCase ) )
				continue; // Fast early bail

			queue.Add( file );
		}

		foreach ( var subDirectory in Directory.GetDirectories( directory ) )
			QueueDirectory( queue, subDirectory );
	}
}
