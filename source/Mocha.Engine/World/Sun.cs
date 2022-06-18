using Veldrid;

namespace Mocha.Engine;

public class Sun : Entity
{
	public SceneLight SceneLight { get; set; }

	public float Intensity
	{
		get => SceneLight.Intensity;
		set => SceneLight.Intensity = value;
	}

	public RgbaFloat Color
	{
		get => SceneLight.Color;
		set => SceneLight.Color = value;
	}

	public Sun()
	{
		SceneLight = new( this );
	}
}
