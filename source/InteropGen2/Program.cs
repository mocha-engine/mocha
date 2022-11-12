public static class Program
{
	internal static List<string> GeneratedPaths { get; set; } = new();
	internal static List<IUnit> Units { get; set; } = new();

	private static void ProcessHeader( string baseDir, string path )
	{
		Console.WriteLine( $"\t Processing header {path}" );

		var fileContents = File.ReadAllText( path );

		if ( !fileContents.Contains( "//@InteropGen" ) )
			return; // Fast early bail

		var units = Parser.GetUnits( path );
		var fileName = Path.GetFileNameWithoutExtension( path );

		var managedGenerator = new ManagedCodeGenerator( units );
		var managedCode = managedGenerator.GenerateManagedCode();
		File.WriteAllText( $"{baseDir}/Common/Glue/{fileName}.generated.cs", managedCode );

		var nativeGenerator = new NativeCodeGenerator( units );
		var relativePath = Path.GetRelativePath( "Host/", path );
		var nativeCode = nativeGenerator.GenerateNativeCode( relativePath );

		Console.WriteLine( $"{baseDir}/Host/generated/{fileName}.generated.h" );
		File.WriteAllText( $"{baseDir}/Host/generated/{fileName}.generated.h", nativeCode );

		Units.AddRange( units );
	}

	private static void ProcessDirectory( string baseDir, string directoryPath )
	{
		foreach ( var file in Directory.GetFiles( directoryPath ) )
		{
			if ( file.EndsWith( ".h" ) && !file.EndsWith( ".generated.h" ) )
			{
				ProcessHeader( baseDir, file );
			}
		}

		foreach ( var subDirectory in Directory.GetDirectories( directoryPath ) )
		{
			ProcessDirectory( baseDir, subDirectory );
		}
	}

	public static void Main( string[] args )
	{
		Console.WriteLine( "Generating C# <--> C++ interop code..." );

		var destCsDir = $"{args[0]}\\Common\\Glue\\";
		var destHeaderDir = $"{args[0]}\\Host\\generated\\";

		if ( Directory.Exists( destHeaderDir ) )
			Directory.Delete( destHeaderDir, true );
		if ( Directory.Exists( destCsDir ) )
			Directory.Delete( destCsDir, true );

		Directory.CreateDirectory( destHeaderDir );
		Directory.CreateDirectory( destCsDir );

		ProcessDirectory( args[0], args[0] );

		// Expand methods out into list of (method name, method)
		var methods = Units.SelectMany( unit => unit.Methods, ( unit, method ) => (unit.Name, method) ).ToList();

		//
		// Write managed struct
		//
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

			File.WriteAllText( $"{args[0]}/Common/Glue/UnmanagedArgs.cs", baseManagedStructWriter.ToString() );
		}

		//
		// Write native struct
		//
		{
			var (baseNativeStructWriter, nativeStructWriter) = Utils.CreateWriter();

			nativeStructWriter.WriteLine( "#ifndef __GENERATED_UNMANAGED_ARGS_H" );
			nativeStructWriter.WriteLine( "#define __GENERATED_UNMANAGED_ARGS_H" );
			nativeStructWriter.WriteLine( "#include \"InteropList.generated.h\"" );
			nativeStructWriter.WriteLine();
			nativeStructWriter.WriteLine( "struct UnmanagedArgs" );
			nativeStructWriter.WriteLine( $"{{" );
			nativeStructWriter.Indent++;

			var nativeStructBody = string.Join( ",\r\n\t", methods.Select( x => $"void* __{x.Name}_{x.method.Name}MethodPtr" ) );
			nativeStructWriter.Write( nativeStructBody );
			nativeStructWriter.WriteLine();

			nativeStructWriter.Indent--;
			nativeStructWriter.WriteLine( $"}};" );
			nativeStructWriter.WriteLine();

			nativeStructWriter.WriteLine( "inline UnmanagedArgs args" );
			nativeStructWriter.WriteLine( $"{{" );
			nativeStructWriter.Indent++;

			nativeStructBody = string.Join( ",\r\n\t", methods.Select( x => $"(void*)__{x.Name}_{x.method.Name}MethodPtr" ) );
			nativeStructWriter.Write( nativeStructBody );
			nativeStructWriter.WriteLine();

			nativeStructWriter.Indent--;
			nativeStructWriter.WriteLine( $"}};" );

			nativeStructWriter.WriteLine();
			nativeStructWriter.WriteLine( $"#endif // __GENERATED_UNMANAGED_ARGS_H" );
			nativeStructWriter.Dispose();

			File.WriteAllText( $"{args[0]}/Host/generated/UnmanagedArgs.generated.h", baseNativeStructWriter.ToString() );
		}

		//
		// Write native includes
		//
		{
			var (baseNativeListWriter, nativeListWriter) = Utils.CreateWriter();

			nativeListWriter.WriteLine( "#ifndef __GENERATED_INTEROPLIST_H" );
			nativeListWriter.WriteLine( "#define __GENERATED_INTEROPLIST_H" );
			nativeListWriter.WriteLine();
			nativeListWriter.Indent++;

			var nativeListBody = string.Join( "\r\n\t", Units.Select( x => $"#include \"{x.Name}.generated.h\"" ) );
			nativeListWriter.Write( nativeListBody );
			nativeListWriter.WriteLine();

			nativeListWriter.Indent--;
			nativeListWriter.WriteLine();
			nativeListWriter.WriteLine( "#endif // __GENERATED_INTEROPLIST_H" );

			File.WriteAllText( $"{args[0]}/Host/generated/InteropList.generated.h", baseNativeListWriter.ToString() );
		}
	}
}
