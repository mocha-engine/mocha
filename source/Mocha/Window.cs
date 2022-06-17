using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Mocha.Renderer;

public class Window
{
	public static Window Current { get; set; }

	public Sdl2Window SdlWindow { get; private set; }

	public Point2 Size => new Point2( SdlWindow.Width, SdlWindow.Height );

	public Window()
	{
		Current ??= this;

		var windowCreateInfo = new WindowCreateInfo()
		{
			WindowWidth = 1280,
			WindowHeight = 720,
			WindowTitle = "Mocha",
			X = 128,
			Y = 128
		};

		SdlWindow = VeldridStartup.CreateWindow( windowCreateInfo );

		Screen.UpdateFrom( Size );
	}
}
