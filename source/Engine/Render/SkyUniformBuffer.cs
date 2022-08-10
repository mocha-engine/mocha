using System.Runtime.InteropServices;

namespace Mocha.Renderer;

[StructLayout( LayoutKind.Sequential )]
public struct SkyUniformBuffer
{
	/*
	 * These fields are padded so that they're
	 * aligned (as blocks) to multiples of 16.
	 */

	public Matrix4x4 g_mModel; // 64
	public Matrix4x4 g_mViewProj; // 64

	public System.Numerics.Vector3 g_vLightPos; // 12
	public float g_flTime; // 4

	public System.Numerics.Vector4 g_vLightColor; // 16

	public System.Numerics.Vector3 g_vCameraPos; // 12
	public float g_flPlanetRadius; // 4

	public System.Numerics.Vector3 g_vSunPos; // 12
	public float g_flAtmosphereRadius; // 4

	public float g_flSunIntensity; // 4
}
