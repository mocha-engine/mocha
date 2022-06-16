global using static Global;
using Veldrid;

namespace Mocha;

/// <summary>
/// Program entry point
/// </summary>
public class Program
{
	public static void Main( string[] args )
	{
		//if ( Veldrid.RenderDoc.Load( out var rd ) )
		{
			//Log.Trace( $"Attached to renderdoc" );
		}

		var game = new Game();
	}
}
