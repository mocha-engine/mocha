using System.Reflection;

namespace MochaTool.AssetCompiler;

class ShaderParser : BaseParser
{
	public ShaderParser( string input ) : base( input )
	{
	}

	private string ParseSectionName()
	{
		ConsumeWhitespace();

		var sectionName = ConsumeWhile( x => !char.IsWhiteSpace( x ) && x != '{' );

		return sectionName;
	}

	private string ParseSectionContent()
	{
		int depth = 0;

		ConsumeWhitespace();

		var sectionContent = ConsumeWhile( x =>
		{
			if ( x == '{' )
			{
				depth++;
			}
			else if ( x == '}' )
			{
				depth--;
			}

			return depth > 0;
		} );

		ConsumeWhitespace();

		ConsumeChar(); // '}'

		// Remove { } characters
		if ( sectionContent.Length >= 2 )
			sectionContent = sectionContent[1..^1];

		return sectionContent;
	}

	public ShaderSourceFile Parse()
	{
		var shaderFile = new ShaderSourceFile();

		while ( !EndOfFile() )
		{
			ConsumeWhitespace();

			var sectionName = ParseSectionName();
			var sectionContent = ParseSectionContent();

			var sectionField = shaderFile.GetType().GetFields().FirstOrDefault( x => x.GetCustomAttribute<ShaderSectionAttribute>()?.SectionName == sectionName );

			if ( sectionField != null )
				sectionField.SetValue( shaderFile, sectionContent );
			else
				Log.Error( $"Unknown section name: {sectionName}" );

			ConsumeWhitespace();
		}

		return shaderFile;
	}
}
