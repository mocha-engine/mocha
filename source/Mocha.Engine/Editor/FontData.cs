using System.Text.Json.Serialization;
namespace Mocha.Engine.Editor;

public class Atlas
{
	[JsonPropertyName( "type" )]
	public string Type { get; set; }

	[JsonPropertyName( "distanceRange" )]
	public int DistanceRange { get; set; }

	[JsonPropertyName( "size" )]
	public float Size { get; set; }

	[JsonPropertyName( "width" )]
	public int Width { get; set; }

	[JsonPropertyName( "height" )]
	public int Height { get; set; }

	[JsonPropertyName( "yOrigin" )]
	public string YOrigin { get; set; }
}

public class Bounds
{
	[JsonPropertyName( "left" )]
	public float Left { get; set; }

	[JsonPropertyName( "bottom" )]
	public float Bottom { get; set; }

	[JsonPropertyName( "right" )]
	public float Right { get; set; }

	[JsonPropertyName( "top" )]
	public float Top { get; set; }
}

public class Glyph
{
	[JsonPropertyName( "unicode" )]
	public int Unicode { get; set; }

	[JsonPropertyName( "advance" )]
	public float Advance { get; set; }

	[JsonPropertyName( "planeBounds" )]
	public Bounds PlaneBounds { get; set; }

	[JsonPropertyName( "atlasBounds" )]
	public Bounds AtlasBounds { get; set; }
}

public class Metrics
{
	[JsonPropertyName( "emSize" )]
	public int EmSize { get; set; }

	[JsonPropertyName( "lineHeight" )]
	public float LineHeight { get; set; }

	[JsonPropertyName( "ascender" )]
	public float Ascender { get; set; }

	[JsonPropertyName( "descender" )]
	public float Descender { get; set; }

	[JsonPropertyName( "underlineY" )]
	public float UnderlineY { get; set; }

	[JsonPropertyName( "underlineThickness" )]
	public float UnderlineThickness { get; set; }
}

public class FontData
{
	[JsonPropertyName( "atlas" )]
	public Atlas Atlas { get; set; }

	[JsonPropertyName( "metrics" )]
	public Metrics Metrics { get; set; }

	[JsonPropertyName( "glyphs" )]
	public List<Glyph> Glyphs { get; set; }

	[JsonPropertyName( "kerning" )]
	public List<object> Kerning { get; set; }
}
