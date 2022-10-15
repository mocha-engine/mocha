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

[Flags]
public enum RoundingFlags
{
	None = 0,
	TopLeft = 1,
	TopRight = 2,
	BottomLeft = 4,
	BottomRight = 8,

	All = TopLeft | TopRight | BottomLeft | BottomRight
}
