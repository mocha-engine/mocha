﻿using System.Runtime.InteropServices;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Mocha.Renderer;

public class Window
{
	public static Window Current { get; set; }
	public Sdl2Window SdlWindow { get; private set; }
	public Point2 Size => new Point2( SdlWindow.Width, SdlWindow.Height );

	public string Title
	{
		set => Sdl2Native.SDL_SetWindowTitle( SdlWindow.SdlWindowHandle, value );
	}

	public Window()
	{
		Current ??= this;

		var windowCreateInfo = new WindowCreateInfo()
		{
			WindowWidth = 1280,
			WindowHeight = 720,
			WindowTitle = "Mocha",
			X = 128,
			Y = 128,
			WindowInitialState = WindowState.Maximized
		};

		SdlWindow = VeldridStartup.CreateWindow( windowCreateInfo );
		SetDarkMode( true );

		Screen.UpdateFrom( Size );
	}

	public void SetDarkMode( bool darkMode )
	{
		SetDarkModeTitlebar( darkMode );
	}

	[DllImport( "dwmapi.dll" )]
	public static extern int DwmSetWindowAttribute( IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute );

	private void SetDarkModeTitlebar( bool darkMode )
	{
		var value = darkMode ? 1 : 0;
		const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
		DwmSetWindowAttribute( SdlWindow.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, Marshal.SizeOf( typeof( int ) ) );
	}
}
