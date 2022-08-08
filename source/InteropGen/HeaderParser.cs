using System.Text.RegularExpressions;

namespace Mocha.InteropGen;

internal partial class HeaderParser : BaseParser
{
	public HeaderParser( string baseDir, string path, string input ) : base( input )
	{
		Input = Regex.Replace( Input, @"//(?!@InteropGen).*", "" );
		Input = Regex.Replace( Input, @"/\*(.|\n)*?\*/", "", RegexOptions.Singleline );

		Input = Input.Replace( "\r\n", "\n" );
	}

	public List<Class> ParseFile()
	{
		var classList = new List<Class>();

		while ( !EndOfFile() )
		{
			if ( ReadUntilHint( out var @class ) )
				classList.Add( @class );
		}

		return classList;
	}

	private bool ReadUntilHint( out Class @class )
	{
		ConsumeWhile( x => !StartsWith( "//@InteropGen" ) );
		var hint = ConsumeWhile( x => x != '\n' );
		hint = hint.Replace( "//@InteropGen ", "" );

		if ( hint.StartsWith( "generate class" ) )
		{
			@class = ReadClass();
			return true;
		}

		@class = default;
		return false;
	}

	private string ReadClassName( out bool isNamespace )
	{
		Assert( StartsWith( "class " ) || StartsWith( "namespace " ) );
		isNamespace = StartsWith( "namespace " );

		ConsumeWhile( x => char.IsLetter( x ) );
		ConsumeWhitespace();

		return ConsumeWhile( x => x != '{' && !char.IsWhiteSpace( x ) );
	}

	private Function? ReadFunctionSignature()
	{
		ConsumeWhitespace();

		if ( StartsWith( "inline" ) || StartsWith( "static" ) )
			ConsumeWhile( x => x != ' ' );

		ConsumeWhitespace();

		var preamble = ConsumeWhile( x => x != '(' && x != ';' ).Replace( "\r\n", "" ).Replace( "\n", "" );
		bool isFunction = NextChar() == '(';

		if ( !isFunction )
		{
			Assert( ConsumeChar() == ';' );
			return null;
		}

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

			void AddParameter()
			{
				if ( !string.IsNullOrEmpty( currentParameter ) )
				{
					var splitParameter = currentParameter.Split( ' ' );
					var parameterType = string.Join( " ", splitParameter[..^1] );
					var parameterName = splitParameter[^1];
					args.Add( new VariableType( parameterType, parameterName ) );

					currentParameter = "";
				}
			}

			if ( nextChar == ',' )
			{
				AddParameter();
				ConsumeChar();
				ConsumeWhitespace();
			}
			else if ( StartsWithIgnoreWhitespace( ");" ) || StartsWithIgnoreWhitespace( "){" ) )
			{
				ConsumeWhitespace();
				AddParameter();
				break;
			}
			else
			{
				currentParameter += ConsumeChar();
			}
		} while ( true );

		ConsumeChar();
		ConsumeWhitespace();

		var consumedChar = ConsumeChar();
		Assert( consumedChar == ';' || consumedChar == '{' );

		if ( consumedChar == '{' )
		{
			var str = ConsumeWhile( x => x != '}' );
			ConsumeChar();
		}

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

		List<string> flags = new();

		while ( NextChar() != '}' )
		{
			ConsumeWhitespace();

			if ( StartsWith( "public:" ) || StartsWith( "private:" ) || StartsWith( "protected:" ) )
				ConsumeWhile( x => x != '\n' );

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

			var function = ReadFunctionSignature();
			if ( function.HasValue )
			{
				var functionCopy = function.Value;
				functionCopy.Flags = flags;

				if ( functionCopy.Type.Name == className || functionCopy.Type.Name == $"~{className}" )
				{
					var type = functionCopy.Type;
					type.NativeType = functionCopy.Type.Name == className ? $"{className}*" : "void";
					type.Name = functionCopy.Type.Name == className ? $"Create" : "Delete";

					functionCopy.IsConstructor = functionCopy.Type.Name == className;
					functionCopy.IsDestructor = functionCopy.Type.Name == $"~{className}";
					functionCopy.Type = type;
				}

				if ( !flags.Contains( "ignore" ) )
					values.Add( functionCopy );
			}

			flags = new();

			ConsumeWhitespace();
		}

		return values;
	}

	private Class ReadClass()
	{
		ConsumeWhitespace();

		var className = ReadClassName( out var isNamespace );

		ConsumeWhitespace();

		Assert( ConsumeChar() == '{' );
		ConsumeWhitespace();

		var classFunctions = ReadClassFunctions( className );
		Assert( ConsumeChar() == '}' );

		return new Class
		{
			IsStatic = isNamespace,
			Name = className,
			Functions = classFunctions
		};
	}
}
