namespace Mocha.InteropGen;

public class Program
{
	internal static IWriter CsStructWriter { get; set; }
	internal static IWriter CppStructWriter { get; set; }

	internal static List<string> GeneratedPaths { get; set; }
	internal static List<Function> Functions { get; set; }

	private static void ProcessHeader( string baseDir, string headerPath )
	{
		Console.WriteLine( $"\t Processing header {headerPath}" );

		var fileContents = File.ReadAllText( headerPath );

		if ( !fileContents.Contains( "//@InteropGen" ) )
			return; // Fast early bail

		var headerParser = new HeaderParser( baseDir, headerPath, fileContents );

		var classes = headerParser.ParseFile();
		CodeGenerator.GenerateCode( headerPath, baseDir, classes );
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

		GeneratedPaths = new();
		Functions = new();

		var destCsDir = $"{args[0]}\\Common\\Glue\\";
		var destHeaderDir = $"{args[0]}\\Mocha\\generated\\";

		Directory.Delete( destHeaderDir, true );
		Directory.Delete( destCsDir, true );

		CsStructWriter = new FileWriter( $"{args[0]}\\Common\\Glue\\UnmanagedArgs.cs" );
		CppStructWriter = new FileWriter( $"{args[0]}\\Mocha\\generated\\UnmanagedArgs.generated.h" );

		CppStructWriter.WriteLine( "#ifndef __GENERATED_UNMANAGED_ARGS_H" );
		CppStructWriter.WriteLine( "#define __GENERATED_UNMANAGED_ARGS_H" );
		CppStructWriter.WriteLine( "#include \"InteropList.generated.h\"" );
		CppStructWriter.WriteLine();
		CppStructWriter.WriteLine( "struct UnmanagedArgs" );
		CppStructWriter.WriteLine( $"{{" );

		CsStructWriter.WriteLine( $"using System.Runtime.InteropServices;" );
		CsStructWriter.WriteLine();
		CsStructWriter.WriteLine( $"[StructLayout( LayoutKind.Sequential )]" );
		CsStructWriter.WriteLine( $"public struct UnmanagedArgs" );
		CsStructWriter.WriteLine( $"{{" );

		ProcessDirectory( args[0], args[0] );

		CsStructWriter.WriteLine( $"}}" );
		CsStructWriter.Dispose();

		CppStructWriter.WriteLine( $"}};" );
		CppStructWriter.WriteLine();

		CppStructWriter.WriteLine( "inline UnmanagedArgs args" );
		CppStructWriter.WriteLine( $"{{" );

		for ( int i = 0; i < Functions.Count; i++ )
		{
			Function function = Functions[i];
			CppStructWriter.Write( $"    (void*)__{function.Class.Name}_{function.Type.Name}" );

			if ( i < Functions.Count - 1 )
				CppStructWriter.WriteLine( "," );
			else
				CppStructWriter.WriteLine();
		}

		CppStructWriter.WriteLine( $"}};" );

		CppStructWriter.WriteLine();
		CppStructWriter.WriteLine( $"#endif // __GENERATED_UNMANAGED_ARGS_H" );
		CppStructWriter.Dispose();

		using ( var cppListWriter = new FileWriter( $"{args[0]}\\Mocha\\generated\\InteropList.generated.h" ) )
		{
			cppListWriter.WriteLine( "#ifndef __GENERATED_INTEROPLIST_H" );
			cppListWriter.WriteLine( "#define __GENERATED_INTEROPLIST_H" );
			cppListWriter.WriteLine();

			foreach ( var generatedPath in GeneratedPaths )
			{
				cppListWriter.WriteLine( $"#include \"{generatedPath}\"" );
			}

			cppListWriter.WriteLine();
			cppListWriter.WriteLine( "#endif // __GENERATED_INTEROPLIST_H" );
		}
	}
}
