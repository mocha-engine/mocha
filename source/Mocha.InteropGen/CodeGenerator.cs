using System.Text;

namespace Mocha.InteropGen;

public class CodeGenerator
{
	private static string GenerateCWrapperFunction( Class @class, Function function )
	{
		var args = function.GetArgsWithInstance( @class );
		var argStr = string.Join( ", ", args.Select( x =>
		{
			if ( x.CSharpType == "string" )
			{
				return $"const char* {x.Name}";
			}

			return $"{x.NativeType} {x.Name}";
		} ) );

		var returnType = function.Type.NativeType;
		if ( function.Type.CSharpType == "string" )
			returnType = "char*";

		var functionSignature = $"extern \"C\" inline {returnType} __{@class.Name}_{function.Type.Name}( {argStr} )";
		var functionBody = "";
		var functionArgs = string.Join( ", ", function.Args.Select( x => x.Name ) );

		if ( function.IsConstructor )
		{
			functionBody += $"return new {@class.Name}( {functionArgs} );";
		}
		else if ( function.IsDestructor )
		{
			functionBody += $"instance->~{@class.Name}( {functionArgs} );";
		}
		else
		{
			var accessor = (@class.IsStatic) ? $"{@class.Name}::" : "instance->";

			if ( function.Type.NativeType == "void" )
				functionBody += $"{accessor}{function.Type.Name}( {functionArgs} );";
			else if ( function.Type.NativeType == "std::string" )
				functionBody += $"std::string text = {accessor}{function.Type.Name}( {functionArgs} );\r\nconst char* cstr = text.c_str();\r\nchar* dup = _strdup(cstr);\r\nreturn dup;";
			else
				functionBody += $"return {accessor}{function.Type.Name}( {functionArgs} );";
		}

		return $"{functionSignature}\r\n{{\r\n\t{functionBody}\r\n}};";
	}

	private static string GenerateCsFunctionDelegate( Class @class, Function function )
	{
		var args = function.GetArgsWithInstance( @class );
		var argStr = string.Join( ", ", args.Select( x => "IntPtr" ).Append( function.Type.CSharpType == "void" ? "void" : "IntPtr" ) );

		return $"delegate* unmanaged< {argStr} >";
	}

	private static string GenerateCsFunctionVariable( Class @class, Function function )
	{
		return $"{GenerateCsFunctionDelegate( @class, function )} _{function.Type.Name}";
	}

	private static void GenerateForClass( string path, string baseDir, Class @class )
	{
		string isStatic = @class.IsStatic ? " static" : "";
		Console.WriteLine( $"\t\tGenerating{isStatic} class {@class.Name}" );

		//
		// Write C header file
		//
		var headerDir = Path.GetDirectoryName( path );
		var destHeaderPath = $"{headerDir}\\generated\\{@class.Name}.generated.h";
		using ( IWriter writer = new FileWriter( destHeaderPath ) )
		{
			StringBuilder functionDecls = new();

			foreach ( var function in @class.Functions )
			{
				functionDecls.AppendLine( GenerateCWrapperFunction( @class, function ) );
			}

			var template = new Template( "Templates/Cpp/CppHeader.template" );
			var parsedTemplate = template.Parse( new()
			{
				{ "ClassName", @class.Name },
				{ "HeaderPath", Path.GetFileNameWithoutExtension( path ) },
				{ "FunctionDecls", functionDecls.ToString() },
			} );

			writer.Write( parsedTemplate );
		}

		//
		// Write c# file
		//
		var destCsPath = baseDir + $"Mocha.Serializer\\Glue\\{@class.Name}.generated.cs";
		using ( IWriter writer = new FileWriter( destCsPath ) )
		{
			StringBuilder functionDecls = new();
			StringBuilder functionDefs = new();
			StringBuilder ctorArgs = new();
			StringBuilder ctorCallArgs = new();
			StringBuilder functionBodies = new();
			StringBuilder ctor = new();

			foreach ( var function in @class.Functions )
			{
				if ( function.Flags.Contains( "ignore" ) )
					continue;

				if ( @class.IsStatic )
					functionDecls.AppendLine( $"\tprivate static {GenerateCsFunctionVariable( @class, function )} = ({GenerateCsFunctionDelegate( @class, function )})Mocha.Common.Global.UnmanagedArgs.__{@class.Name}_{function.Type.Name}MethodPtr;" );
				else
					functionDecls.AppendLine( $"\tprivate {GenerateCsFunctionVariable( @class, function )};" );
			}

			var createFunction = @class.Functions.FirstOrDefault( x => x.Type.Name == "Create" );
			var createFunctionArgs = createFunction.Args?.Select( x => $"InteropUtils.GetPtr( {x.Name} )" ) ?? new string[0];
			ctorCallArgs.Append( string.Join( ", ", createFunctionArgs ) );

			ctorArgs.Append( string.Join( ", ", createFunction.Args?.Select( x =>
				string.Join( " ", new[] { x.CSharpMarshalAs, x.CSharpType, x.Name } )
			) ?? new string[0] ) );

			foreach ( var function in @class.Functions )
			{
				functionDefs.AppendLine( $"\t\tthis._{function.Type.Name} = ({GenerateCsFunctionDelegate( @class, function )})args.__{@class.Name}_{function.Type.Name}MethodPtr;" );
			}

			foreach ( var function in @class.Functions )
			{
				var args = function.Args;
				var argStr = string.Join( ", ", args.Select( x => $"{x.CSharpType} {x.Name}" ) );

				var callArgs = function.GetArgsWithInstance( @class );
				var methodCallArgs = string.Join( ", ", callArgs.Select( x => $"InteropUtils.GetPtr( {x.Name} )" ) );

				functionBodies.AppendLine();
				functionBodies.AppendLine( $"\tpublic {isStatic} {function.Type.CSharpType} {function.Type.Name}( {argStr} )" );
				functionBodies.AppendLine( $"\t{{" );

				var accessor = "";

				if ( !@class.IsStatic )
				{
					accessor += "this.";
				}

				if ( function.Type.CSharpType == "void" )
					functionBodies.AppendLine( $"\t\t{accessor}_{function.Type.Name}( {methodCallArgs} );" );
				else if ( function.Type.CSharpType == "bool" )
					functionBodies.AppendLine( $"\t\treturn {accessor}_{function.Type.Name}( {methodCallArgs} ).ToInt32() != 0;" );
				else if ( function.Type.CSharpType == "int" )
					functionBodies.AppendLine( $"\t\treturn {accessor}_{function.Type.Name}( {methodCallArgs} ).ToInt32();" );
				else if ( function.Type.CSharpType == "long" )
					functionBodies.AppendLine( $"\t\treturn {accessor}_{function.Type.Name}( {methodCallArgs} ).ToInt64();" );
				else if ( function.Type.CSharpType == "string" )
					functionBodies.AppendLine( $"\t\treturn InteropUtils.GetString( {accessor}_{function.Type.Name}( {methodCallArgs} ) );" );
				else
					functionBodies.AppendLine( $"\t\treturn ({function.Type.CSharpType}){accessor}_{function.Type.Name}( {methodCallArgs} );" );

				functionBodies.AppendLine( $"\t}}" );
			}

			Template template = new Template( "Templates/Cs/CsClass.template" );

			if ( @class.IsStatic )
				template = new Template( "Templates/Cs/CsStaticClass.template" );

			if ( @class.Functions.Any( x => x.IsConstructor ) )
				ctor.AppendLine( $"this.instance = this._Create( {ctorCallArgs.ToString()} );" );

			var parsedTemplate = template.Parse( new()
			{
				{ "ClassName", @class.Name },
				{ "Ctor", ctor.ToString() },
				{ "FunctionDecls", functionDecls.ToString() },
				{ "CtorArgs", ctorArgs.ToString() },
				{ "FunctionDefs", functionDefs.ToString() },
				{ "CtorCallArgs", ctorCallArgs.ToString() },
				{ "FunctionBodies", functionBodies.ToString() },
			} );

			writer.Write( parsedTemplate );
		}

		//
		// Write to C# struct
		//
		foreach ( var function in @class.Functions )
		{
			Program.CsStructWriter.WriteLine( $"    public IntPtr __{@class.Name}_{function.Type.Name}MethodPtr;" );
		}

		//
		// Write to C++ struct
		//
		foreach ( var function in @class.Functions )
		{
			Program.CppStructWriter.WriteLine( $"    void* __{@class.Name}_{function.Type.Name}MethodPtr;" );
		}

		//
		// Add to global func list
		//
		foreach ( var function in @class.Functions )
		{
			var functionCopy = function;
			functionCopy.Class = @class;
			Program.Functions.Add( functionCopy );
		}

		//
		// Add to global class list
		//
		Program.GeneratedPaths.Add( destHeaderPath );
	}

	public static void GenerateCode( string path, string baseDir, List<Class> classes )
	{
		Console.WriteLine( $"\t\tGenerating classes..." );
		foreach ( var @class in classes )
		{
			GenerateForClass( path, baseDir, @class );
		}
	}
}
