namespace Mocha.Engine;

[Category( "World" ), Icon( FontAwesome.CloudSun ), Title( "Sky" )]
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
			Shader = ShaderBuilder.Default.FromPath( "core/shaders/atmosphere.mshdr" ).Build(),
			UniformBufferType = typeof( SkyUniformBuffer )
		};

		SceneObject = new SkySceneObject()
		{
			models = new() { Primitives.Cube.GenerateModel( Material ) }
		};
	}

	public override void Update()
	{
		base.Update();
		
		SceneObject.Transform = Transform.WithPosition( Vector3.Zero );
	}
}
