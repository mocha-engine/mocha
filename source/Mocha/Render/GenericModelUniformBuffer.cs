using System.Numerics;
using System.Runtime.InteropServices;

namespace Mocha.Renderer;

[StructLayout( LayoutKind.Sequential )]
public struct GenericModelUniformBuffer
{
	/*
	 * These fields are padded so that they're
	 * aligned (as blocks) to multiples of 16.
	 */

	public Matrix4x4 g_mModel; // 64
	public Matrix4x4 g_mViewProj; // 64
	public Matrix4x4 g_mPadding; // 64
	public Matrix4x4 g_mLightViewProj;

	public System.Numerics.Vector3 g_vSunLightDir;
	public float g_flTime; // 4

	public System.Numerics.Vector3 g_vLightPos; // 12
	public float g_flSunLightIntensity; // 4

	public System.Numerics.Vector3 g_vCameraPos; // 12
	public float _padding1; // 4

	public System.Numerics.Vector3 g_vLightColor; // 12
	public float _padding2; // 4

	public System.Numerics.Vector4 g_vSunLightColor; // 16
}
