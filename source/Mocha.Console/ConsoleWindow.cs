using Veldrid;
using Veldrid.StartupUtilities;
using ImGuiNET;
using Mocha.Console;

public class ConsoleWindow
{
	DateTime lastUpdate = DateTime.Now;
	List<ConsoleInstance> instances = new List<ConsoleInstance> { new ConsoleInstance() };

	private void DrawMenuBar()
	{
		ImGui.BeginMainMenuBar();

		ImGui.Dummy( new( 4, 0 ) );
		ImGui.Text( "Mocha Console" );
		ImGui.Dummy( new( 4, 0 ) );

		ImGui.Separator();
		ImGui.Dummy( new( 4, 0 ) );

		if ( ImGui.BeginMenu( "File" ) )
		{
			if ( ImGui.MenuItem( "Connect" ) )
			{
				instances.Add( new() );
			}

			ImGui.EndMenu();
		}

		ImGui.EndMainMenuBar();
	}

	public void Run()
	{
		VeldridStartup.CreateWindowAndGraphicsDevice(
			new WindowCreateInfo( 50, 50, 1024, 768, WindowState.Normal, "Mocha Console" ),
			out var window,
			out var graphicsDevice );

		graphicsDevice.SyncToVerticalBlank = true;

		var cl = graphicsDevice.ResourceFactory.CreateCommandList();

		var imguiRenderer = new ImGuiRenderer(
			graphicsDevice,
			graphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
			window.Width,
			window.Height );

		window.Resized += () =>
		{
			imguiRenderer.WindowResized( window.Width, window.Height );
			graphicsDevice.MainSwapchain.Resize( (uint)window.Width, (uint)window.Height );
		};

		window.Closed += () =>
		{
			Environment.Exit( 0 );
		};

		var io = ImGui.GetIO();
		io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
		Theme.Set();

		while ( window.Exists )
		{
			var snapshot = window.PumpEvents();

			var delta = (DateTime.Now - lastUpdate).TotalSeconds;
			imguiRenderer.Update( (float)delta, snapshot );
			lastUpdate = DateTime.Now;

			DrawMenuBar();

			var viewport = ImGui.GetMainViewport();
			ImGui.SetNextWindowSize( viewport.WorkSize );
			ImGui.SetNextWindowPos( viewport.WorkPos );
			ImGui.SetNextWindowViewport( viewport.ID );
			ImGui.PushStyleVar( ImGuiStyleVar.WindowRounding, 0 );

			if ( ImGui.Begin( "Console", ImGuiWindowFlags.NoDecoration ) )
			{
				if ( ImGui.BeginTabBar( "consoleTabs", ImGuiTabBarFlags.Reorderable ) )
				{
					foreach ( ConsoleInstance instance in instances.ToArray() )
					{
						bool isOpen = true;

						if ( ImGui.BeginTabItem( $"{instance.Title}##{instance.GetHashCode()}", ref isOpen ) )
						{
							instance.Render();
							ImGui.EndTabItem();
						}

						if ( !isOpen )
						{
							instances.Remove( instance );
						}
					}

					ImGui.EndTabBar();
				}
			}

			ImGui.PopStyleVar();
			ImGui.End();

			cl.Begin();
			cl.SetFramebuffer( graphicsDevice.MainSwapchain.Framebuffer );
			cl.ClearColorTarget( 0, new RgbaFloat( 0, 0, 0, 1f ) );

			imguiRenderer.Render( graphicsDevice, cl );

			cl.End();
			graphicsDevice.SubmitCommands( cl );
			graphicsDevice.SwapBuffers( graphicsDevice.MainSwapchain );
		}
	}
}
