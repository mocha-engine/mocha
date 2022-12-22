﻿namespace Mocha.UI;

public class Node
{
	public List<Node> Children { get; protected set; }
}

public class TextNode : Node
{
	public TextNode( string text )
	{
		Text = text;
	}

	public string Text { get; set; }
}

public class ElementData
{
	public string TagName { get; set; }
	public Dictionary<string, string> Attributes { get; set; }

	public string Id => Attributes.GetValueOrDefault( "id" ) ?? "";
	public List<string> Class => (Attributes.GetValueOrDefault( "class" ) ?? "").Split( ' ' ).ToList();
}

public class ElementNode : Node
{
	public ElementData Data { get; set; }

	public ElementNode( string tagName, Dictionary<string, string> attributes, List<Node> children )
	{
		Children = children;

		Data = new()
		{
			Attributes = attributes,
			TagName = tagName
		};
	}
}
