using Veldrid;

namespace Mocha;

public class Sun : Entity
{
	public float Intensity { get; set; } = 1.0f;
	public RgbaFloat Color { get; set; } = new RgbaFloat( 1, 1, 1, 1 );
}
