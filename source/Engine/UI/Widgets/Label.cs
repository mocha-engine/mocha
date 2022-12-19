namespace Mocha.UI;

internal class Label : Widget
{
	private string calculatedText = "";
	private string text;

	public string Text
	{
		get => text; set
		{
			text = value;
			CalculateText();
		}
	}

	public Vector4 Color { get; set; } = Theme.TextColor;
	public float FontSize { get; set; } = Theme.FontSize;
	public string FontFamily { get; set; } = Theme.Font;

	internal Label( string text, float fontSize = Theme.FontSize, string fontFamily = Theme.Font ) : base()
	{
		FontSize = fontSize;
		Text = text;
		FontFamily = fontFamily;
	}

	internal override void Render()
	{
		Graphics.DrawText( Bounds, calculatedText, FontFamily, FontSize );
	}

	internal override Vector2 GetDesiredSize()
	{
		return Graphics.MeasureText( Text, FontFamily, FontSize );
	}

	internal override void OnBoundsChanged()
	{
		CalculateText();
	}

	private void CalculateText()
	{
		var text = Text;
		calculatedText = text;
	}
}
