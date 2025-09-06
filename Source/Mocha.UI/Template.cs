using Mocha.Common;

namespace Mocha.UI;

public static class Template
{
	public static LayoutNode FromFile( IRenderer renderer, string path )
	{
		var templatePath = Path.ChangeExtension( path, ".html" );
		var stylePath = Path.ChangeExtension( path, ".scss" );

		while ( !FileSystem.Mounted.IsFileReady( templatePath ) ) ;
		while ( !FileSystem.Mounted.IsFileReady( stylePath ) ) ;

		var rootTemplateNode = HtmlParser.Parse( FileSystem.Mounted.ReadAllText( templatePath ) );
		var stylesheet = ScssParser.Parse( FileSystem.Mounted.ReadAllText( stylePath ) );
		var rootStyledNode = StyleTree.BuildTree( rootTemplateNode, stylesheet );

		var rootLayoutNode = LayoutTree.BuildTree( renderer, rootStyledNode );

		return rootLayoutNode;
	}
}
