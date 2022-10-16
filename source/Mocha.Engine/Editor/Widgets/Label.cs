namespace Mocha.Engine.Editor;

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

	public Vector4 Color { get; set; } = ITheme.Current.TextColor;
	public float FontSize { get; set; } = 12;
	public string FontFamily { get; set; } = "sourcesanspro";

	internal Label( string text, float fontSize = 12, string fontFamily = "sourcesanspro" ) : base()
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
