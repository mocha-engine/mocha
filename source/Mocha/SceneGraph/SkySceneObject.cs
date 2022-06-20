namespace Mocha.Renderer;

public class SkySceneObject : ModelSceneObject
{
	public float SunIntensity { get; set; } = 32.0f;
	public float PlanetRadius { get; set; } = 6371000;
	public float AtmosphereRadius { get; set; } = 6381000;

	internal Framebuffer ShadowBuffer { get; set; }
	internal Texture ShadowTexture { get; set; }
	internal Texture ColorTexture { get; set; }

	public SkySceneObject( IEntity entity ) : base( entity )
	{
		ShadowTexture = Texture.Builder
			.FromEmpty( 1024, 1024 )
			.Build();

		ColorTexture = Texture.Builder
			.FromEmpty( 1024, 1024 )
			.Build();

		var framebufferDescription = new FramebufferDescription( ShadowTexture.VeldridTexture, ColorTexture.VeldridTexture );
		ShadowBuffer = Device.ResourceFactory.CreateFramebuffer( framebufferDescription );
	}

	public override void Render( CommandList commandList )
	{
		var currentCamera = SceneWorld.Current.Camera;

		var uniformBuffer = new SkyUniformBuffer
		{
			g_mModel = ModelMatrix,
			g_mView = SceneWorld.Current.Camera.ViewMatrix,
			g_mProj = SceneWorld.Current.Camera.ProjMatrix,
			g_vLightPos = SceneWorld.Current.Sun.Transform.Position,
			g_flTime = Time.Now,
			g_vLightColor = SceneWorld.Current.Sun.Color.ToVector4(),
			g_vCameraPos = SceneWorld.Current.Camera.Transform.Position,

			g_flPlanetRadius = PlanetRadius,
			g_flAtmosphereRadius = AtmosphereRadius,
			g_flSunIntensity = SunIntensity,
			g_vSunPos = SceneWorld.Current.Sun.Transform.Rotation.Backward
		};

		models.ForEach( x => x.Draw( uniformBuffer, commandList ) );
	}
}
