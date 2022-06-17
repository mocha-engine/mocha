﻿using Veldrid;
using Veldrid.StartupUtilities;

namespace Mocha.Renderer;

public class RendererInstance
{
	public Window window;

	private SceneWorld world;
	private DateTime lastFrame;

	private CommandList commandList;

	public RendererInstance()
	{
		Init();

		lastFrame = DateTime.Now;
		MainLoop();
	}

	private void Init()
	{
		window = new();

		CreateGraphicsDevice();
		commandList = Device.ResourceFactory.CreateCommandList();

		world = new();
	}

	private void MainLoop()
	{
		while ( window.SdlWindow.Exists )
		{
			Update();

			PreRender();
			Render();
			PostRender();
		}
	}

	private void PreRender()
	{
		commandList.Begin();
		commandList.SetFramebuffer( Device.SwapchainFramebuffer );
		commandList.ClearColorTarget( 0, RgbaFloat.Black );
		commandList.ClearDepthStencil( 1 );
	}

	private void PostRender()
	{
		commandList.End();
		Device.SyncToVerticalBlank = true;
		Device.SubmitCommands( commandList );
		Device.SwapBuffers();
	}

	private void Render()
	{
		world.Render( commandList );
	}

	private void Update()
	{
		float deltaTime = (float)(DateTime.Now - lastFrame).TotalSeconds;
		lastFrame = DateTime.Now;

		Time.UpdateFrom( deltaTime );
		Input.Update();

		world.Update();
	}

	private void CreateGraphicsDevice()
	{
		var options = new GraphicsDeviceOptions()
		{
			PreferStandardClipSpaceYDirection = true,
			PreferDepthRangeZeroToOne = true,
			SwapchainDepthFormat = PixelFormat.D24_UNorm_S8_UInt,
			SwapchainSrgbFormat = false,
			SyncToVerticalBlank = false
		};

		var preferredBackend = GraphicsBackend.Vulkan;
		Device = VeldridStartup.CreateGraphicsDevice( Window.Current.SdlWindow, options, preferredBackend );

		var windowTitle = $"Mocha | {Device.BackendType}";
		Window.Current.SdlWindow.Title = windowTitle;
	}

	public void OnWindowResized( Point2 newSize )
	{
		Device.MainSwapchain.Resize( (uint)newSize.X, (uint)newSize.Y );
	}
}