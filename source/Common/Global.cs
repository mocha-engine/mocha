global using Vector3 = Mocha.Common.Vector3;
global using Matrix4x4 = System.Numerics.Matrix4x4;
global using Vector4 = System.Numerics.Vector4;

namespace Mocha.Common;

public static class Global
{
	public static Logger Log { get; } = new();
	public static UnmanagedArgs UnmanagedArgs { get; set; }
}

