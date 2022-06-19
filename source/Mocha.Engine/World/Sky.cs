namespace Mocha.Engine;

public class Sky : Entity
{
	public SkySceneObject SceneObject { get; set; }
	private Material Material { get; set; }

	public float SunIntensity
	{
		get => SceneObject.SunIntensity;
		set => SceneObject.SunIntensity = value;
	}

	public float PlanetRadius
	{
		get => SceneObject.PlanetRadius;
		set => SceneObject.PlanetRadius = value;
	}

	public float AtmosphereRadius
	{
		get => SceneObject.AtmosphereRadius;
		set => SceneObject.AtmosphereRadius = value;
	}

	public Sky()
	{
		Material = new()
		{
			Shader = ShaderBuilder.Default.FromMoyaiShader( "content/shaders/atmosphere.mshdr" ).Build(),
			UniformBufferType = typeof( SkyUniformBuffer )
		};

		SceneObject = new SkySceneObject( this )
		{
			models = new() { Primitives.Cube.GenerateModel( Material ) }
		};
	}
}
