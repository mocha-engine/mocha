using FlexLayoutSharp;
using YogaNode = FlexLayoutSharp.Node;

namespace Mocha.UI;

public class LayoutNode
{
	public YogaNode YogaNode { get; set; }
	public StyledNode StyledNode { get; set; }
	public Rectangle Bounds { get; set; }
	public LayoutNode Parent { get; set; }
	public List<LayoutNode> Children { get; set; } = new();
	public string ComposedName { get; set; }

	public bool IsRootNode => this.Parent == null;
}

public class LayoutTree
{
	private YogaNode GetYogaNode( IRenderer renderer, StyledNode node, LayoutNode parent = null )
	{
		var yogaNode = Flex.CreateDefaultNode();

		float? width = node.StyleValues.Width?.Value;
		float? height = node.StyleValues.Height?.Value;

		if ( node.Node is TextNode textNode )
		{
			var fontSize = node.Parent.StyleValues.FontSize?.Value ?? 16;
			var weight = node.Parent.StyleValues.FontWeight?.Value ?? 400;
			var fontFamily = node.Parent.StyleValues.FontFamily?.Value ?? "Inter";

			var textSize = renderer.CalcTextSize( textNode.Text, fontFamily, (int)weight, fontSize );

			width = textSize.X;
			height = textSize.Y;
		}

		if ( parent == null )
		{
			yogaNode.StyleSetPositionType( PositionType.Relative );
			yogaNode.StyleSetWidth( Screen.Size.X / Screen.UIScale );
			yogaNode.StyleSetHeight( 1080 );
		}
		else
		{
			if ( width != null )
				yogaNode.StyleSetWidth( width ?? 0 );
			else
				yogaNode.StyleSetWidthAuto();

			if ( height != null )
				yogaNode.StyleSetHeight( height ?? 0 );
			else
				yogaNode.StyleSetHeightAuto();

			yogaNode.StyleSetMaxWidth( node.StyleValues.MaxWidth?.Value ?? float.NaN );
			yogaNode.StyleSetMaxHeight( node.StyleValues.MaxHeight?.Value ?? float.NaN );

			yogaNode.StyleSetMinWidth( node.StyleValues.MinWidth?.Value ?? float.NaN );
			yogaNode.StyleSetMinHeight( node.StyleValues.MinHeight?.Value ?? float.NaN );

			yogaNode.StyleSetPosition( Edge.Top, node.StyleValues.Top?.Value ?? float.NaN );
			yogaNode.StyleSetPosition( Edge.Left, node.StyleValues.Left?.Value ?? float.NaN );
			yogaNode.StyleSetPosition( Edge.Right, node.StyleValues.Right?.Value ?? float.NaN );
			yogaNode.StyleSetPosition( Edge.Bottom, node.StyleValues.Bottom?.Value ?? float.NaN );
			yogaNode.StyleSetPositionType( node.StyleValues.Position?.GetValue<PositionType>() ?? PositionType.Relative );
		}

		yogaNode.StyleSetPadding( Edge.Top, node.StyleValues.PaddingTop?.Value ?? float.NaN );
		yogaNode.StyleSetPadding( Edge.Left, node.StyleValues.PaddingLeft?.Value ?? float.NaN );
		yogaNode.StyleSetPadding( Edge.Right, node.StyleValues.PaddingRight?.Value ?? float.NaN );
		yogaNode.StyleSetPadding( Edge.Bottom, node.StyleValues.PaddingBottom?.Value ?? float.NaN );
		yogaNode.StyleSetPadding( Edge.All, node.StyleValues.Padding?.Value ?? float.NaN );

		yogaNode.StyleSetMargin( Edge.Top, node.StyleValues.MarginTop?.Value ?? float.NaN );
		yogaNode.StyleSetMargin( Edge.Left, node.StyleValues.MarginLeft?.Value ?? float.NaN );
		yogaNode.StyleSetMargin( Edge.Right, node.StyleValues.MarginRight?.Value ?? float.NaN );
		yogaNode.StyleSetMargin( Edge.Bottom, node.StyleValues.MarginBottom?.Value ?? float.NaN );
		yogaNode.StyleSetMargin( Edge.All, node.StyleValues.Margin?.Value ?? float.NaN );

		yogaNode.StyleSetFlexGrow( node.StyleValues.FlexGrow?.Value ?? 0 );
		yogaNode.StyleSetFlexShrink( node.StyleValues.FlexShrink?.Value ?? 0 );

		yogaNode.StyleSetAspectRatio( node.StyleValues.AspectRatio?.Value ?? float.NaN );

		yogaNode.StyleSetAlignItems( node.StyleValues.AlignItems?.GetValue<Align>() ?? Align.Stretch );
		yogaNode.StyleSetJustifyContent( node.StyleValues.JustifyContent?.GetValue<Justify>() ?? Justify.FlexStart );
		yogaNode.StyleSetFlexDirection( node.StyleValues.FlexDirection?.GetValue<FlexDirection>() ?? FlexDirection.Row );
		yogaNode.StyleSetDisplay( node.StyleValues.Display?.GetValue<Display>() ?? Display.Flex );
		yogaNode.StyleSetFlexWrap( node.StyleValues.FlexWrap?.GetValue<Wrap>() ?? Wrap.NoWrap );

		if ( parent != null )
			parent.YogaNode.InsertChild( yogaNode, parent.YogaNode.ChildrenCount );

		return yogaNode;
	}

	public static LayoutNode BuildTree( IRenderer renderer, StyledNode styledNode, LayoutNode parent = null )
	{
		var layoutTree = new LayoutTree();
		var layoutNode = new LayoutNode();

		layoutNode.StyledNode = styledNode;
		layoutNode.Parent = parent;
		layoutNode.YogaNode = layoutTree.GetYogaNode( renderer, styledNode, parent );

		if ( styledNode.Children != null )
			layoutNode.Children = styledNode.Children.Select( x => BuildTree( renderer, x, layoutNode ) ).ToList();

		if ( parent == null )
		{
			// this is the root node
			Flex.CalculateLayout( layoutNode.YogaNode, float.NaN, float.NaN, Direction.LTR );

			void ApplyLayout( LayoutNode applyNode )
			{
				Rectangle initialPosition = new();

				if ( applyNode.Parent != null )
				{
					initialPosition = applyNode.Parent.Bounds;

					applyNode.Bounds = new Rectangle(
						initialPosition.X + applyNode.YogaNode.LayoutGetLeft(),
						initialPosition.Y + applyNode.YogaNode.LayoutGetTop(),
						applyNode.YogaNode.LayoutGetWidth(),
						applyNode.YogaNode.LayoutGetHeight()
					);
				}

				foreach ( var applyChild in applyNode.Children )
					ApplyLayout( applyChild );
			}

			ApplyLayout( layoutNode );
		}

		return layoutNode;
	}
}
