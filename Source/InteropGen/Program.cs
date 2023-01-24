namespace MochaTool.InteropGen;

public static class Program
{
	internal static List<string> GeneratedPaths { get; set; } = new();
	internal static List<IUnit> Units { get; set; } = new();
	internal static List<string> Files { get; set; } = new();

	private static void ProcessHeader( string baseDir, string path )
	{
		Console.WriteLine( $"\t Processing header {path}" );

		var units = Parser.GetUnits( path );
		var fileName = Path.GetFileNameWithoutExtension( path );

		var managedGenerator = new ManagedCodeGenerator( units );
		var managedCode = managedGenerator.GenerateManagedCode();
		File.WriteAllText( $"{baseDir}/Common/Glue/{fileName}.generated.cs", managedCode );

		var nativeGenerator = new NativeCodeGenerator( units );
		var relativePath = Path.GetRelativePath( $"{baseDir}/Host/", path );
		var nativeCode = nativeGenerator.GenerateNativeCode( relativePath );

		Console.WriteLine( $"{baseDir}/Host/generated/{fileName}.generated.h" );
		File.WriteAllText( $"{baseDir}/Host/generated/{fileName}.generated.h", nativeCode );

		Files.Add( fileName );
		Units.AddRange( units );
	}

	private static void QueueDirectory( ref List<string> queue, string directory )
	{
		foreach ( var file in Directory.GetFiles( directory ) )
		{
			if ( file.EndsWith( ".h" ) && !file.EndsWith( ".generated.h" ) )
			{
				var fileContents = File.ReadAllText( file );

				if ( !fileContents.Contains( "GENERATE_BINDINGS", StringComparison.CurrentCultureIgnoreCase ) )
					continue; // Fast early bail

				QueueFile( ref queue, file );
			}
		}

		foreach ( var subDirectory in Directory.GetDirectories( directory ) )
		{
			QueueDirectory( ref queue, subDirectory );
		}
	}

	private static void QueueFile( ref List<string> queue, string path )
	{
		queue.Add( path );
	}

	private static void Parse( string baseDir )
	{
		List<string> queue = new();
		QueueDirectory( ref queue, baseDir );

		var dispatcher = new ThreadDispatcher<string>( ( files ) =>
		{
			foreach ( var path in files )
			{
				ProcessHeader( baseDir, path );
			}
		}, queue );

		// Wait for all threads to finish...
		while ( !dispatcher.IsComplete )
			Thread.Sleep( 500 );
	}

	private static void WriteManagedStruct( string baseDir, ref List<(string Name, Method method)> methods )
	{
		var (baseManagedStructWriter, managedStructWriter) = Utils.CreateWriter();

		managedStructWriter.WriteLine( $"using System.Runtime.InteropServices;" );
		managedStructWriter.WriteLine();
		managedStructWriter.WriteLine( $"[StructLayout( LayoutKind.Sequential )]" );
		managedStructWriter.WriteLine( $"public struct UnmanagedArgs" );
		managedStructWriter.WriteLine( $"{{" );

		managedStructWriter.Indent++;

		var managedStructBody = string.Join( "\r\n\t", methods.Select( x => $"public IntPtr __{x.Name}_{x.method.Name}MethodPtr;" ) );
		managedStructWriter.Write( managedStructBody );
		managedStructWriter.WriteLine();

		managedStructWriter.Indent--;

		managedStructWriter.WriteLine( $"}}" );
		managedStructWriter.Dispose();

		File.WriteAllText( $"{baseDir}/Common/Glue/UnmanagedArgs.cs", baseManagedStructWriter.ToString() );
	}

	private static void WriteNativeStruct( string baseDir, ref List<(string Name, Method method)> methods )
	{
		var (baseNativeStructWriter, nativeStructWriter) = Utils.CreateWriter();

		nativeStructWriter.WriteLine( "#ifndef __GENERATED_UNMANAGED_ARGS_H" );
		nativeStructWriter.WriteLine( "#define __GENERATED_UNMANAGED_ARGS_H" );
		nativeStructWriter.WriteLine( "#include \"InteropList.generated.h\"" );
		nativeStructWriter.WriteLine();
		nativeStructWriter.WriteLine( "struct UnmanagedArgs" );
		nativeStructWriter.WriteLine( $"{{" );
		nativeStructWriter.Indent++;

		var nativeStructBody = string.Join( "\r\n\t", methods.Select( x => $"void* __{x.Name}_{x.method.Name}MethodPtr;" ) );
		nativeStructWriter.Write( nativeStructBody );
		nativeStructWriter.WriteLine();

		nativeStructWriter.Indent--;
		nativeStructWriter.WriteLine( $"}};" );
		nativeStructWriter.WriteLine();

		nativeStructWriter.WriteLine( "inline UnmanagedArgs args" );
		nativeStructWriter.WriteLine( $"{{" );
		nativeStructWriter.Indent++;

		nativeStructBody = string.Join( ",\r\n\t", methods.Select( x => $"(void*)__{x.Name}_{x.method.Name}" ) );
		nativeStructWriter.Write( nativeStructBody );
		nativeStructWriter.WriteLine();

		nativeStructWriter.Indent--;
		nativeStructWriter.WriteLine( $"}};" );

		nativeStructWriter.WriteLine();
		nativeStructWriter.WriteLine( $"#endif // __GENERATED_UNMANAGED_ARGS_H" );
		nativeStructWriter.Dispose();

		File.WriteAllText( $"{baseDir}/Host/generated/UnmanagedArgs.generated.h", baseNativeStructWriter.ToString() );
	}

	private static void WriteNativeIncludes( string baseDir )
	{
		var (baseNativeListWriter, nativeListWriter) = Utils.CreateWriter();

		nativeListWriter.WriteLine( "#ifndef __GENERATED_INTEROPLIST_H" );
		nativeListWriter.WriteLine( "#define __GENERATED_INTEROPLIST_H" );
		nativeListWriter.WriteLine();
		nativeListWriter.Indent++;

		var nativeListBody = string.Join( "\r\n\t", Files.Select( x => $"#include \"{x}.generated.h\"" ) );
		nativeListWriter.Write( nativeListBody );
		nativeListWriter.WriteLine();

		nativeListWriter.Indent--;
		nativeListWriter.WriteLine();
		nativeListWriter.WriteLine( "#endif // __GENERATED_INTEROPLIST_H" );

		File.WriteAllText( $"{baseDir}/Host/generated/InteropList.generated.h", baseNativeListWriter.ToString() );
	}

	private static void DeleteExistingFiles( string baseDir )
	{
		var destCsDir = $"{baseDir}\\Common\\Glue\\";
		var destHeaderDir = $"{baseDir}\\Host\\generated\\";

		if ( Directory.Exists( destHeaderDir ) )
			Directory.Delete( destHeaderDir, true );
		if ( Directory.Exists( destCsDir ) )
			Directory.Delete( destCsDir, true );

		Directory.CreateDirectory( destHeaderDir );
		Directory.CreateDirectory( destCsDir );
	}

	public static void Main( string[] args )
	{
		var baseDir = args[0];
		var start = DateTime.Now;

		Console.WriteLine( "Generating C# <--> C++ interop code..." );

		//
		// Prep
		//
		DeleteExistingFiles( baseDir );
		Parse( baseDir );

		//
		// Expand methods out into list of (method name, method)
		//
		var methods = Units.OfType<Class>().SelectMany( unit => unit.Methods, ( unit, method ) => (unit.Name, method) ).ToList();

		//
		// Write files
		//
		WriteManagedStruct( baseDir, ref methods );
		WriteNativeStruct( baseDir, ref methods );
		WriteNativeIncludes( baseDir );

		// Track time & output total duration
		var end = DateTime.Now;
		var totalTime = end - start;
		Console.WriteLine( $"-- Took {totalTime.TotalSeconds} seconds." );
	}
}
