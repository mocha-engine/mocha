using Mocha.Common.World;

namespace Mocha.Renderer;

public class ModelSceneObject : SceneObject
{
	public Model model;

	public Matrix4x4 ModelMatrix => Transform.BuildMatrix();

	public ModelSceneObject()
	{
	}

	public override void Render( Matrix4x4 viewProjMatrix, RenderPass renderPass, CommandList commandList )
	{
		var currentCamera = SceneWorld.Current.Camera;

		var uniformBuffer = new GenericModelUniformBuffer
		{
			g_mModel = ModelMatrix,
			g_mViewProj = viewProjMatrix,
			g_flTime = Time.Now,
			g_mLightViewProj = SceneWorld.Current.Sun.ViewMatrix * SceneWorld.Current.Sun.ProjMatrix,

			g_vSunLightDir = -SceneWorld.Current.Sun.ViewMatrix.Forward(),
			g_vSunLightColor = SceneWorld.Current.Sun.Color.ToVector4(),
			g_flSunLightIntensity = SceneWorld.Current.Sun.Intensity,
			g_vCameraPos = currentCamera.Transform.Position,

			_padding1 = 0,
			_padding2 = 0
		};

		model.Meshes.ForEach( x => x.Draw( renderPass, uniformBuffer, commandList ) );
	}
}
