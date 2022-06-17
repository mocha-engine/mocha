global using Mocha;
global using Veldrid;
global using Mocha.Common;
global using Matrix4x4 = System.Numerics.Matrix4x4;
global using Vector4 = System.Numerics.Vector4;

namespace Mocha.Common;

public static class Global
{
	public static Logger Log { get; } = new();
}

