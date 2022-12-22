using Mocha.Common;

namespace Mocha.UI;

public static class Template
{
	public static LayoutNode FromFile( IRenderer renderer, string path )
	{
		var templatePath = Path.ChangeExtension( path, ".html" );
		var stylePath = Path.ChangeExtension( path, ".scss" );

		while ( !FileSystem.Game.IsFileReady( templatePath ) ) ;
		while ( !FileSystem.Game.IsFileReady( stylePath ) ) ;

		var rootTemplateNode = HtmlParser.Parse( FileSystem.Game.ReadAllText( templatePath ) );
		var stylesheet = ScssParser.Parse( FileSystem.Game.ReadAllText( stylePath ) );
		var rootStyledNode = StyleTree.BuildTree( rootTemplateNode, stylesheet );

		var rootLayoutNode = LayoutTree.BuildTree( renderer, rootStyledNode );

		return rootLayoutNode;
	}
}
