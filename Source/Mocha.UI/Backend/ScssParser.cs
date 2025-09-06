using System.Reflection;
using System.Text.RegularExpressions;

namespace Mocha.UI;

class ScssParser : BaseParser
{
	public ScssParser( string input ) : base( input )
	{
	}

	private bool IsValidIdentifier( char c )
	{
		return char.IsDigit( c ) || char.IsLetter( c );
	}

	private string ParseIdentifier()
	{
		return ConsumeWhile( x => char.IsDigit( x ) || char.IsLetter( x ) || x == '-' || x == '_' );
	}

	private Selector ParseSelector()
	{
		var selector = new Selector()
		{
			TagName = "",
			Id = "",
			Class = new()
		};

		while ( !EndOfFile() )
		{
			if ( NextChar() == '#' )
			{
				ConsumeChar();
				selector.Id = ParseIdentifier();
			}
			if ( NextChar() == '.' )
			{
				ConsumeChar();
				selector.Class.Add( ParseIdentifier() );
			}
			if ( NextChar() == '*' )
			{
				ConsumeChar();
			}
			if ( NextChar() == ':' )
			{
				ConsumeChar();
				selector.PseudoClass = ParseIdentifier();
			}
			if ( IsValidIdentifier( NextChar() ) )
			{
				selector.TagName = ParseIdentifier();
			}
			else
			{
				break;
			}
		}

		return selector;
	}

	private StyleValues ParseDeclaration( StyleValues styleValues )
	{
		var name = ConsumeWhile( x => x != ':' );
		ConsumeChar();
		ConsumeWhitespace();

		var value = "";

		while ( !EndOfFile() )
		{
			if ( NextChar() == ';' )
				break;

			value += ConsumeChar();
		}

		foreach ( var property in typeof( StyleValues ).GetProperties() )
		{
			var styleAttribute = property.GetCustomAttribute<StylePropertyAttribute>();
			if ( styleAttribute == null )
				continue;

			if ( styleAttribute.CssName == name )
			{
				var parseMethod = property.PropertyType.GetMethod( "ParseFrom" );
				var parsedValue = parseMethod?.Invoke( null, new string[] { value } );

				property.SetValue( styleValues, parsedValue );
				break;
			}
		}

		return styleValues;
	}

	private List<Selector> ParseSelectors()
	{
		var selectors = new List<Selector>();

		while ( !EndOfFile() )
		{
			selectors.Add( ParseSelector() );
			ConsumeWhitespace();

			if ( NextChar() == ',' )
			{
				ConsumeChar();
				ConsumeWhitespace();
			}
			else if ( NextChar() == '{' )
			{
				ConsumeChar();
				break;
			}
			else
			{
				// Unknown selector, bail immediately
				Log.Error( $"Couldn't parse selector '{NextChar()}'" );
				return selectors;
			}
		}

		// TODO: Specificity

		return selectors;
	}

	private StyleValues ParseDeclarations()
	{
		var declarations = new StyleValues();
		while ( !EndOfFile() )
		{
			if ( NextChar() == '}' )
			{
				ConsumeChar();
				break;
			}

			declarations = ParseDeclaration( declarations );
			ConsumeWhitespace();

			if ( NextChar() == ';' )
			{
				ConsumeChar();
				ConsumeWhitespace();
			}
		}

		return declarations;
	}

	private Rule ParseRule()
	{
		var rule = new Rule();

		ConsumeWhitespace();
		rule.Selectors = ParseSelectors();

		ConsumeWhitespace();
		rule.StyleValues = ParseDeclarations();

		return rule;
	}
	private List<Rule> ParceRules()
	{
		var rules = new List<Rule>();
		ConsumeWhitespace();

		while ( !EndOfFile() )
		{
			rules.Add( ParseRule() );
		}

		return rules;
	}

	public static Stylesheet Parse( string input )
	{
		// Filter comments from input
		input = Regex.Replace( input, @"(\/\/)(.+?)(?=[\n\r]|\*\))", "" );

		// Find all scss variable declarations
		// Examples:
		// $dark-50: #E3E3E3;
		// $dark-100: #D9D9D9;

		var variableDeclarations = Regex.Matches( input, @"(\$[a-zA-Z0-9_\-]+):\s*(.+?);" );

		foreach ( Match variable in variableDeclarations )
		{
			var variableName = variable.Groups[1].Value;
			var variableValue = variable.Groups[2].Value;

			// Remove declaration
			input = input.Replace( variable.Value, "" );

			// Replace with css variable
			input = input.Replace( variableName + ";", variableValue + ";" );
		}

		// Find any definitions, remove them
		input = Regex.Replace( input, $@"\$.*:\s+.*;", "" );

		var rules = new ScssParser( input ).ParceRules();

		var stylesheet = new Stylesheet()
		{
			Rules = rules
		};

		return stylesheet;
	}
}
