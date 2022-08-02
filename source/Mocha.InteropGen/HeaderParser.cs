using System.Text.RegularExpressions;

namespace Mocha.InteropGen;

internal class HeaderParser : BaseParser
{
	struct VariableType
	{
		public string Name { get; set; }
		public string NativeType { get; set; }
		public string CSharpType
		{
			get
			{
				if ( NativeType == "const char*" || NativeType == "string_t" )
				{
					return "[MarshalAs( UnmanagedType.LPStr )] string";
				}
				else if ( NativeType.EndsWith( "*" ) )
				{
					return "IntPtr";
				}
				else
				{
					return NativeType;
				}
			}
		}

		public VariableType( string nativeType, string name )
		{
			Name = name;
			NativeType = nativeType;
		}
	}

	struct Function
	{
		public List<string> Flags { get; set; } = new();
		public VariableType Type { get; set; } = new( "void", "Unnamed" );
		public List<VariableType> Args { get; set; } = new();
		public bool PassClassInstance { get; set; } = true;

		public List<VariableType> GetArgsWithInstance( string className )
		{
			var args = new List<VariableType>();

			if ( PassClassInstance )
				args.Add( new VariableType( $"{className}*", "instance" ) );

			args.AddRange( Args );

			return args;
		}

		public Function() { }

		public override string ToString()
		{
			return Type.Name;
		}
	}

	public HeaderParser( string input ) : base( input )
	{
		Input = Regex.Replace( Input, @"//(?!@InteropGen).*", "" );
		Input = Regex.Replace( Input, @"/\*(.|\n)*?\*/", "", RegexOptions.Singleline );

		Input = Input.Replace( "\r\n", "\n" );

		while ( !EndOfFile() )
			ReadUntilHint();
	}

	private void ReadUntilHint()
	{
		ConsumeWhile( x => !StartsWith( "//@InteropGen" ) );
		var hint = ConsumeWhile( x => x != '\n' );
		hint = hint.Replace( "//@InteropGen ", "" );

		if ( hint.StartsWith( "generate class" ) )
		{
			ReadClass();
		}
	}

	private string ReadClassName()
	{
		Assert( StartsWith( "class " ) );
		ConsumeWhile( x => char.IsLetter( x ) );

		ConsumeWhitespace();

		return ConsumeWhile( x => x != '{' && !char.IsWhiteSpace( x ) );
	}

	private Function ReadFunctionSignature()
	{
		ConsumeWhitespace();

		var preamble = ConsumeWhile( x => x != '(' );
		ConsumeChar();
		ConsumeWhitespace();

		var splitPreamble = preamble.Split( ' ' );
		var functionName = splitPreamble[^1];
		var returnType = string.Join( ' ', splitPreamble[..^1] );

		List<VariableType> args = new();
		string currentParameter = "";

		ConsumeWhitespace();

		char nextChar = '\0';

		do
		{
			nextChar = NextChar();

			if ( nextChar == ',' || nextChar == ')' )
			{
				if ( !string.IsNullOrEmpty( currentParameter ) )
				{
					var splitParameter = currentParameter.Split( ' ' );
					var parameterType = string.Join( " ", splitParameter[..^1] );
					var parameterName = splitParameter[^1];
					args.Add( new VariableType( parameterType, parameterName ) );

					currentParameter = "";
					ConsumeChar();
					ConsumeWhitespace();
				}

				if ( nextChar == ')' )
					break;
			}
			else
			{
				currentParameter += ConsumeChar();
			}
		} while ( true );

		Assert( ConsumeChar() == ';' );
		ConsumeWhitespace();

		return new Function
		{
			Type = new VariableType( returnType, functionName ),
			Args = args
		};
	}

	private List<Function> ReadClassFunctions( string className )
	{
		List<Function> values = new();

		if ( StartsWith( "public:" ) || StartsWith( "private:" ) || StartsWith( "protected:" ) )
			ConsumeWhile( x => x != '\n' );

		List<string> flags = new();

		while ( NextChar() != '}' )
		{
			ConsumeWhitespace();

			if ( StartsWith( "//@InteropGen" ) )
			{
				var command = ConsumeWhile( x => x != '\n' );
				command = command.Replace( "//@InteropGen ", "" );
				flags.Add( command );
				ConsumeChar();

				continue;
			}

			ConsumeWhitespace();

			if ( StartsWith( className ) )
			{
				// Handle ctor
				ConsumeWhile( x => x != ';' );
				ConsumeChar();

				values.Add( new Function()
				{
					Type = new( $"{className}*", "Create" ),
					PassClassInstance = false
				} );
			}

			ConsumeWhitespace();

			if ( StartsWith( $"~{className}" ) )
			{
				// Handle dtor
				ConsumeWhile( x => x != ';' );
				ConsumeChar();

				values.Add( new Function()
				{
					Type = new( $"void", "Delete" )
				} );
			}

			ConsumeWhitespace();

			var function = ReadFunctionSignature();
			function.Flags = flags;

			values.Add( function );
			flags = new();

			ConsumeWhitespace();
		}

		return values;
	}

	private string GenerateCWrapperHeader( string className, Function function )
	{
		var args = function.GetArgsWithInstance( className );
		var argStr = string.Join( ", ", args.Select( x => $"{x.NativeType} {x.Name}" ) );

		var functionSignature = $"extern \"C\" inline {function.Type.NativeType} __{className}_{function.Type.Name}( {argStr} )";
		var functionBody = "";
		var functionArgs = string.Join( ", ", function.Args.Select( x => x.Name ) );

		if ( function.Type.NativeType == "void" )
			functionBody += $"instance->{function.Type.Name}( {functionArgs} );";
		else
			functionBody += $"return instance->{function.Type.Name}( {functionArgs} );";

		return $"{functionSignature} {{ {functionBody} }};";
	}

	private string GenerateCsFunctionDelegate( string className, Function function )
	{
		var args = function.GetArgsWithInstance( className );
		var argStr = string.Join( ", ", args.Select( x => $"{x.CSharpType} {x.Name}" ) );
		return $"delegate {function.Type.CSharpType} {function.Type.Name}Delegate( {argStr} );";
	}

	private string GenerateCsFunctionVariable( string className, Function function )
	{
		return $"{function.Type.Name}Delegate {function.Type.Name}Method;";
	}

	private void ReadClass()
	{
		ConsumeWhitespace();

		var className = ReadClassName();
		ConsumeWhitespace();

		Assert( ConsumeChar() == '{' );
		ConsumeWhitespace();

		var classFunctions = ReadClassFunctions( className );
		Assert( ConsumeChar() == '}' );

		//
		// Write C header file
		//
		using ( IWriter writer = new ConsoleWriter( $"{className}.generated.h" ) )
		{
			foreach ( var function in classFunctions )
			{
				if ( function.Flags.Contains( "ignore" ) )
					continue;

				writer.WriteLine( $"{GenerateCWrapperHeader( className, function )}" );
			}
		}

		//
		// Write c# file
		//
		using ( IWriter writer = new ConsoleWriter( $"{className}.generated.cs" ) )
		{
			writer.WriteLine( $"public class {className}" );
			writer.WriteLine( $"{{" );
			writer.WriteLine( $"    private IntPtr instance;" );

			foreach ( var function in classFunctions )
			{
				if ( function.Flags.Contains( "ignore" ) )
					continue;

				writer.WriteLine( $"    private {GenerateCsFunctionDelegate( className, function )}" );
				writer.WriteLine( $"    private {GenerateCsFunctionVariable( className, function )}" );
				writer.WriteLine();
			}

			writer.WriteLine( $"    public {className}( UnmanagedArgs args )" );
			writer.WriteLine( $"    {{" );
			writer.WriteLine( $"        this.instance = args.{className}Ptr;" );

			foreach ( var function in classFunctions )
			{
				if ( function.Flags.Contains( "ignore" ) )
					continue;

				writer.WriteLine( $"        this.{function.Type.Name}Method = Marshal.GetDelegateForFunctionPointer<{function.Type.Name}Delegate>( args.{function.Type.Name}MethodPtr );" );
			}

			writer.WriteLine( $"    }}" );

			foreach ( var function in classFunctions )
			{
				if ( function.Flags.Contains( "ignore" ) )
					continue;

				var args = function.Args;
				var argStr = string.Join( ", ", args.Select( x => $"{x.CSharpType} {x.Name}" ) );

				var callArgs = function.GetArgsWithInstance( className );
				var methodCallArgs = string.Join( ", ", callArgs.Select( x => $"{x.Name}" ) );

				writer.WriteLine();
				writer.WriteLine( $"    public {function.Type.CSharpType} {function.Type.Name}( {argStr} )" );
				writer.WriteLine( $"    {{" );

				if ( function.Type.NativeType == "void" )
					writer.WriteLine( $"        this.{function.Type.Name}Method( {methodCallArgs} );" );
				else
					writer.WriteLine( $"        return this.{function.Type.Name}Method( {methodCallArgs} );" );

				writer.WriteLine( $"    }}" );
			}

			writer.WriteLine( "}" );
		}
	}
}
