using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Mocha;

public class Sky : Entity
{
	private Model Model { get; set; }
	private Material Material { get; set; }

	[StructLayout( LayoutKind.Sequential )]
	struct SkyUniformBuffer
	{
		/*
		 * These fields are padded so that they're
		 * aligned (as blocks) to multiples of 16.
		 */

		public Matrix4x4 g_mModel; // 64
		public Matrix4x4 g_mView; // 64
		public Matrix4x4 g_mProj; // 64

		public System.Numerics.Vector3 g_vLightPos; // 12
		public float g_flTime; // 4

		public System.Numerics.Vector3 g_vLightColor; // 12
		public float g_flSunIntensity; // 4

		public System.Numerics.Vector3 g_vCameraPos; // 12
		public float g_flPlanetRadius; // 4

		public float g_flAtmosphereRadius; // 4
		public float _padding0; // 4
	}

	public float SunIntensity { get; set; } = 32.0f;
	public float PlanetRadius { get; set; } = 6371000;
	public float AtmosphereRadius { get; set; } = 6471000;

	public Sky()
	{
		Material = new()
		{
			Shader = ShaderBuilder.Default.FromMoyaiShader( "content/shaders/atmosphere.mshdr" ).Build(),
			UniformBufferType = typeof( SkyUniformBuffer )
		};

		Model = Primitives.Cube.GenerateModel( Material );
	}

	public override void Render( CommandList commandList )
	{
		var uniformBuffer = new SkyUniformBuffer
		{
			g_mModel = ModelMatrix,
			g_mView = World.Current.Camera.ViewMatrix,
			g_mProj = World.Current.Camera.ProjMatrix,
			g_vLightPos = World.Current.Sun.position,
			g_flTime = Time.Now,
			g_vLightColor = World.Current.Sun.Color,
			g_vCameraPos = World.Current.Camera.position,

			g_flPlanetRadius = PlanetRadius,
			g_flAtmosphereRadius = AtmosphereRadius,
			g_flSunIntensity = SunIntensity,

			_padding0 = 0
		};

		Model.Draw( uniformBuffer, commandList );
	}
}
