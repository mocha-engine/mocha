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

    public override void Render( Matrix4x4 viewProjMatrix, RenderPass renderPass, CommandList commandList )
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

        models.ForEach( x => x.Draw( renderPass, uniformBuffer, commandList ) );
    }
}
