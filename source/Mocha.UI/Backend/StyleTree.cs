namespace Mocha.UI;

public class StyledNode
{
    public Node Node { get; set; }
    public StyleValues StyleValues { get; set; }
    public StyledNode Parent { get; set; }
    public List<StyledNode> Children { get; set; } = new();
    public string InnerText { get; set; }
    public string ComposedName { get; set; }
}

public class StyleTree
{
    private bool MatchesSelector( ElementData elementData, Selector selector )
    {
        if ( !string.IsNullOrEmpty( selector.TagName ) )
            if ( selector.TagName != elementData.TagName )
                return false;

        if ( !string.IsNullOrEmpty( selector.Id ) )
            if ( selector.Id != elementData.Id )
                return false;

        if ( selector.Class.Any( x => !elementData.Class.Contains( x ) ) )
            return false;

        return true;
    }

    private bool Matches( ElementData elementData, Selector selector )
    {
        return MatchesSelector( elementData, selector );
    }

    private Rule MatchRule( ElementData elementData, Rule rule )
    {
        foreach ( var selector in rule.Selectors )
        {
            if ( Matches( elementData, selector ) )
                return rule;
        }

        return null;
    }

    private List<Rule> MatchingRules( ElementData elementData, Stylesheet stylesheet )
    {
        return stylesheet.Rules.Select( x => MatchRule( elementData, x ) ).ToList();
    }

    private StyleValues SpecifiedValues( ElementData element, Stylesheet stylesheet )
    {
        var values = new StyleValues();
        var rules = MatchingRules( element, stylesheet );

        foreach ( var rule in rules )
        {
            if ( rule == null )
                continue;

            values.CombineWith( rule.StyleValues );
        }

        return values;
    }

    private string GetComposedName( StyledNode styledNode )
    {
        if ( styledNode.Node is not ElementNode elementNode )
            return "";

        var composedStr = elementNode.Data.TagName;

        var className = string.Join( ".", elementNode.Data.Class );
        if ( !string.IsNullOrEmpty( className ) )
            composedStr += $".{className}";

        if ( !string.IsNullOrEmpty( elementNode.Data.Id ) )
            composedStr += $"#{elementNode.Data.Id}";

        return composedStr;
    }

    public static StyledNode BuildTree( Node rootNode, Stylesheet stylesheet, StyledNode parent = null )
    {
        var styleTree = new StyleTree();
        var styledNode = new StyledNode();

        styledNode.Node = rootNode;
        styledNode.Parent = parent;
        styledNode.ComposedName = styleTree.GetComposedName( styledNode );

        if ( rootNode is ElementNode elementNode )
        {
            styledNode.StyleValues = styleTree.SpecifiedValues( elementNode.Data, stylesheet );
        }
        else
        {
            if ( rootNode is TextNode textNode )
                styledNode.InnerText = textNode.Text;

            styledNode.StyleValues = new();
        }

        if ( rootNode.Children != null )
            styledNode.Children = rootNode.Children.Select( x => BuildTree( x, stylesheet, styledNode ) ).ToList();

        return styledNode;
    }
}
