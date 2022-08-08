﻿using Mocha.Common.World;
using Veldrid.StartupUtilities;

namespace Mocha.Renderer;

public class RendererInstance
{
	public Window window;

	private SceneWorld world;
	private DateTime lastFrame;

	private CommandList commandList;

	private Material gbufferCombineMaterial;
	private Mesh fullscreenQuad;

	public Action PreUpdate;
	public Action OnUpdate;
	public Action PostUpdate;

	public Action<CommandList> RenderOverlays;

	public RendererInstance()
	{
		Event.Register( this );

		Init();
		lastFrame = DateTime.Now;
	}

	private void Init()
	{
		window = new();

		CreateGraphicsDevice();
		// Swap the buffers so that the screen isn't a mangled mess
		Device.SwapBuffers();

		commandList = Device.ResourceFactory.CreateCommandList();

		world = new();
	}

	public void Run()
	{
		gbufferCombineMaterial = new Material()
		{
			Shader = new ShaderBuilder().FromPath( "core/shaders/combine.mshdr" )
											.WithFramebuffer( Device.SwapchainFramebuffer )
											.WithFaceCullMode( FaceCullMode.None )
											.Build(),
			UniformBufferType = typeof( EmptyUniformBuffer ),
			DiffuseTexture = world.Camera.ColorTexture
		};

		fullscreenQuad = Primitives.Plane.GenerateModel( gbufferCombineMaterial );

		while ( window.SdlWindow.Exists )
		{
			Update();

			// Build the camera right before we render, makes sure we're
			// in the right spot with as little latency as possible
			{
				var worldCameraSetup = BuildCamera();
				worldCameraSetup.BuildMatrices( out var viewMatrix, out var projMatrix );
				RenderPass( Renderer.RenderPass.Main, viewMatrix * projMatrix, world.Camera.Framebuffer );
			}

			PreRender();

			//
			// Render world
			//
			{
				world.Sun.CalcViewProjMatrix();
				RenderPass( Renderer.RenderPass.ShadowMap, world.Sun.ViewMatrix * world.Sun.ProjMatrix, world.Sun.ShadowBuffer );
			}

			PostRender();
		}
	}

	private void PreRender()
	{
		// TODO: Make this nicer
		// Check each shader, if it's dirty then recompile it
		foreach ( var shader in Asset.All.OfType<Shader>().Where( x => x.IsDirty ) )
		{
			shader.Recompile();
		}

		commandList.Begin();
	}

	private CameraSetup BuildCamera()
	{
		var cameraSetup = new CameraSetup();

		world.Camera.BuildCamera( ref cameraSetup );

		foreach ( var entity in EntityPool.Entities )
		{
			entity.BuildCamera( ref cameraSetup );
		}

		return cameraSetup;
	}

	private void PostRender()
	{
		commandList.SetFramebuffer( Device.SwapchainFramebuffer );
		commandList.SetFullViewports();
		commandList.SetFullScissorRects();

		commandList.ClearColorTarget( 0, RgbaFloat.Black );
		commandList.ClearDepthStencil( 1 );

		commandList.PushDebugGroup( "GBuffer Combine" );
		fullscreenQuad.Draw( Renderer.RenderPass.Combine, new EmptyUniformBuffer(), commandList );
		commandList.PopDebugGroup();

		commandList.PushDebugGroup( "UI Render" );
		RenderOverlays?.Invoke( commandList );
		commandList.PopDebugGroup();

		commandList.End();

		Device.SubmitCommands( commandList );
		Device.SwapBuffers();
	}

	private void RenderPass( RenderPass renderPass, Matrix4x4 viewProjMatrix, Framebuffer framebuffer )
	{
		commandList.PushDebugGroup( renderPass.ToString() );
		commandList.SetFramebuffer( framebuffer );
		commandList.SetViewport( 0, new Viewport( 0, 0, framebuffer.Width, framebuffer.Height, 0, 1 ) );
		commandList.SetFullViewports();
		commandList.SetFullScissorRects();

		for ( uint i = 0; i < framebuffer.ColorTargets.Count; ++i )
			commandList.ClearColorTarget( i, RgbaFloat.Black );

		commandList.ClearDepthStencil( 1 );

		world.Render( viewProjMatrix, renderPass, commandList );
		commandList.PopDebugGroup();
	}

	private void Update()
	{
		float deltaTime = (float)(DateTime.Now - lastFrame).TotalSeconds;
		lastFrame = DateTime.Now;

		Time.UpdateFrom( deltaTime );

		PreUpdate?.Invoke();
		OnUpdate?.Invoke();
		PostUpdate?.Invoke();

		if ( Input.Pressed( InputButton.Fullscreen ) )
		{
			Window.Current.Fullscreen = !Window.Current.Fullscreen;
		}
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

		Device = VeldridStartup.CreateGraphicsDevice( Window.Current.SdlWindow, options );
		Window.Current.Title = $"Mocha | {Device.BackendType}";
	}

	[Event.Window.Resized]
	public void OnWindowResized( Point2 newSize )
	{
		Device.MainSwapchain.Resize( (uint)newSize.X, (uint)newSize.Y );
	}
}