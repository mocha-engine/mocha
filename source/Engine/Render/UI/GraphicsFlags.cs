namespace Mocha.Renderer.UI;

[Flags]
public enum GraphicsFlags
{
	None = 0,
	UseRawImage = 1,
	UseSdf = 2,

	HighDistMul = 4,

	RoundedTopLeft = 8,
	RoundedTopRight = 16,
	RoundedBottomLeft = 32,
	RoundedBottomRight = 64,

	Border = 128
}
