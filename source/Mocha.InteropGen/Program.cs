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

		CsStructWriter = new FileWriter( $"{args[0]}\\Mocha.Serializer\\Glue\\UnmanagedArgs.cs" );
		CppStructWriter = new FileWriter( $"{args[0]}\\Mocha.Hostess\\generated\\UnmanagedArgs.generated.h" );

		CppStructWriter.WriteLine( "#pragma once" );
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

		CppStructWriter.WriteLine( "UnmanagedArgs args" );
		CppStructWriter.WriteLine( $"{{" );

		foreach ( var function in Functions )
		{
			CppStructWriter.WriteLine( $"    (void*)__{function.ClassName}_{function.Type.Name}," );
		}

		CppStructWriter.WriteLine( $"}};" );

		using ( var cppListWriter = new FileWriter( $"{args[0]}\\Mocha.Hostess\\generated\\InteropList.generated.h" ) )
		{
			cppListWriter.WriteLine( "#pragma once" );

			foreach ( var generatedPath in GeneratedPaths )
			{
				cppListWriter.WriteLine( $"#include \"{generatedPath}\"" );
			}
		}

		CppStructWriter.Dispose();
	}
}
