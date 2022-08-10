using Mocha.Common.World;

namespace Mocha.Renderer;

public class SkySceneObject : ModelSceneObject
{
	public float SunIntensity { get; set; } = 32.0f;
	public float PlanetRadius { get; set; } = 6372000;
	public float AtmosphereRadius { get; set; } = 6380000;

	public SkySceneObject( IEntity entity ) : base( entity )
	{
	}
}
