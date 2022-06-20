using System.Numerics;

struct ConsoleItem
{
	public Vector4 Color { get; set; }
	public string Text { get; set; }

	public ConsoleItem( Vector4 color, string text )
	{
		Color = color;
		Text = text;
	}
}
