global using static Mocha.Engine.Global;
global using Matrix4x4 = System.Numerics.Matrix4x4;
global using Vector4 = System.Numerics.Vector4;

namespace Mocha.Engine;

/// <summary>
/// Program entry point
/// </summary>
public class Program
{
	public static void Main( string[] args )
	{
#if RENDERDOC
		if ( !Veldrid.RenderDoc.Load( out var rd ) )
		{
			throw new Exception();
		}
#endif

		var game = new Game();
	}
}
