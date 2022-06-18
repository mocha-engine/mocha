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
		if ( Veldrid.RenderDoc.Load( out var rd ) )
		{
			Log.Trace( $"Attached to renderdoc" );
		}

		var game = new Game();
	}
}
