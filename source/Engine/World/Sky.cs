namespace Mocha;

[Category( "World" ), Icon( FontAwesome.CloudSun ), Title( "Sky" )]
public class Sky : BaseEntity
{
	private Material Material { get; set; }

	public float SunIntensity
	{
		get;
		set;
	}

	public float PlanetRadius
	{
		get;
		set;
	}

	public float AtmosphereRadius
	{
		get;
		set;
	}

	public Sky()
	{
		Material = new()
		{
			Shader = ShaderBuilder.Default.FromMoyaiShader( "shaders/atmosphere.mshdr" ).Build(),
			UniformBufferType = typeof( SkyUniformBuffer )
		};
	}
}
