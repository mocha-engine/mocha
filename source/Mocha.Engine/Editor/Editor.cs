using ImGuiNET;
using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid;

namespace Mocha.Engine;

internal partial class Editor
{
	public static Editor Instance { get; private set; }
	public bool ShouldRender { get; set; } = true;
	public static ImFontPtr MonospaceFont { get; private set; }
	public static ImFontPtr SansSerifFont { get; private set; }
	public static ImFontPtr BoldFont { get; private set; }
	public static ImFontPtr HeadingFont { get; private set; }
	public static ImFontPtr SubheadingFont { get; private set; }

	private Renderer.Texture Logo { get; set; }

	private Renderer.Texture defaultFontTexture;
	private List<BaseTab> tabs = new();

	// TODO: I don't like this
	internal RendererInstance Renderer { get; }

	public Editor( RendererInstance renderer )
	{
		Instance ??= this;
		Renderer = renderer;

		Init();
		SetTheme();

		tabs.AddRange( Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where( x => typeof( BaseTab ).IsAssignableFrom( x ) )
			.Select( x => Activator.CreateInstance( x ) )
			.OfType<BaseTab>()
		);

		Logo = TextureBuilder.UITexture.FromPath( "content/logo.png" ).Build();
	}

	private static void AddIconFont( ImGuiIOPtr io )
	{
		unsafe
		{
			var iconConfig = ImGuiNative.ImFontConfig_ImFontConfig();
			iconConfig->MergeMode = 1;
			iconConfig->GlyphMinAdvanceX = 24.0f;

			var iconRanges = new ushort[] { FontAwesome.IconMin, FontAwesome.IconMax, 0 };

			fixed ( ushort* rangePtr = iconRanges )
			{
				io.Fonts.AddFontFromFileTTF( "content/fonts/fa-solid-900.ttf", 12.0f, iconConfig, (IntPtr)rangePtr );
				io.Fonts.AddFontFromFileTTF( "content/fonts/fa-regular-400.ttf", 12.0f, iconConfig, (IntPtr)rangePtr );
			}
		}
	}

	public static Mocha.Renderer.Texture GenerateFontTexture()
	{
		var io = ImGui.GetIO();

		io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.IsSRGB | ImGuiConfigFlags.ViewportsEnable;
		ImGui.LoadIniSettingsFromDisk( ImGui.GetIO().IniFilename ); // https://github.com/mellinoe/veldrid/issues/410

		io.Fonts.Clear();

		SansSerifFont = io.Fonts.AddFontFromFileTTF( "content/fonts/Inter-Regular.ttf", 14f );
		AddIconFont( io );

		BoldFont = io.Fonts.AddFontFromFileTTF( "content/fonts/Inter-Bold.ttf", 14f );
		AddIconFont( io );

		SubheadingFont = io.Fonts.AddFontFromFileTTF( "content/fonts/Inter-Medium.ttf", 20f );
		AddIconFont( io );

		HeadingFont = io.Fonts.AddFontFromFileTTF( "content/fonts/Inter-Bold.ttf", 24f );
		AddIconFont( io );

		MonospaceFont = io.Fonts.AddFontDefault();

		io.Fonts.Build();
		io.Fonts.GetTexDataAsRGBA32( out IntPtr pixels, out var width, out var height, out var bpp );

		int size = width * height * bpp;
		byte[] data = new byte[size];
		Marshal.Copy( pixels, data, 0, size );

		//pinnedIconRanges.Free();
		//Marshal.FreeHGlobal( iconRangesPtr );
		//Marshal.FreeHGlobal( configPtr );

		return TextureBuilder.UITexture.FromData( data, (uint)width, (uint)height ).Build();
	}

	private static void SetKeyMappings( ImGuiIOPtr io )
	{
		io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
		io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
		io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
		io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
		io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
		io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
		io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
		io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
		io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
		io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
		io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.BackSpace;
		io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
		io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
		io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
		io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
		io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
		io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
		io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
		io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
	}

	public void Init()
	{
		var io = ImGui.GetIO();

		var editorFontTexture = GenerateFontTexture();
		var imguiBinding = Renderer.GetImGuiBinding( editorFontTexture );

		io.Fonts.SetTexID( imguiBinding );
		io.Fonts.ClearTexData();

		SetKeyMappings( io );
	}

	private void DrawPerfOverlay()
	{
		var io = ImGui.GetIO();
		var windowFlags = ImGuiWindowFlags.NoDecoration |
			ImGuiWindowFlags.AlwaysAutoResize |
			ImGuiWindowFlags.NoSavedSettings |
			ImGuiWindowFlags.NoFocusOnAppearing |
			ImGuiWindowFlags.NoNav |
			ImGuiWindowFlags.NoInputs |
			ImGuiWindowFlags.NoMove;

		const float padding = 8.0f;

		var viewport = ImGui.GetMainViewport();
		var workPos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!

		System.Numerics.Vector2 windowPos, windowPivot;

		windowPos.X = workPos.X + padding;
		windowPos.Y = workPos.Y + padding;
		windowPivot.X = 0.0f;
		windowPivot.Y = 0.0f;

		ImGui.SetNextWindowPos( windowPos, ImGuiCond.Always, windowPivot );
		ImGui.SetNextWindowBgAlpha( 0.5f );
		ImGui.SetNextWindowSize( new System.Numerics.Vector2( 150, 0 ) );

		if ( ImGui.Begin( $"##overlay", windowFlags ) )
		{
			string total = GC.GetTotalMemory( false ).ToSize( MathX.SizeUnits.MB );

			ImGui.PushFont( HeadingFont );
			ImGui.Text( $"{io.Framerate.CeilToInt()}fps" );
			ImGui.PopFont();

			ImGui.PushFont( SubheadingFont );
			ImGui.Text( $"{total} total" );
			ImGui.PopFont();
		}

		ImGui.Text( $"Running for {Time.Now:0}s" );
		ImGui.Text( $"Frame time {Time.Delta:0.0000}s" );
		ImGui.End();
	}

	private void DrawMenuBar()
	{
		ImGui.PushStyleVar( ImGuiStyleVar.FramePadding, new System.Numerics.Vector2( 0, 16 ) );
		ImGui.BeginMainMenuBar();

		ImGui.Dummy( new( 4, 0 ) );

		ImGui.SetCursorPosY( 8 );
		EditorHelpers.Image( Logo, new Vector2( 32, 32 ) );
		ImGui.SetCursorPosY( 0 );

		ImGui.Dummy( new( 4, 0 ) );
		ImGui.Separator();
		ImGui.Dummy( new( 4, 0 ) );

		foreach ( var tab in tabs )
		{
			var editorMenuAttribute = tab.GetType().GetCustomAttribute<EditorMenuAttribute>();
			if ( editorMenuAttribute == null )
				continue;

			var splitPath = editorMenuAttribute.Path.Split( '/' );

			if ( ImGui.BeginMenu( splitPath[0] ) )
			{
				for ( int i = 1; i < splitPath.Length; i++ )
				{
					string? item = splitPath[i];
					bool active = ImGui.MenuItem( item );

					if ( i == splitPath.Length - 1 && active )
						tab.isVisible = !tab.isVisible;
				}

				ImGui.EndMenu();
			}
		}

		ImGui.PopStyleVar();

		//
		// Buttons
		//
		{
			ImGui.PushStyleVar( ImGuiStyleVar.FramePadding, new System.Numerics.Vector2( 4, 0 ) );
			ImGui.PushStyleColor( ImGuiCol.Button, System.Numerics.Vector4.Zero );

			// Draw play, pause, resume buttons in center
			var center = ImGui.GetMainViewport().WorkSize.X / 2.0f;
			center -= 50f; // Approx.
			ImGui.SetCursorPosX( center );
			ImGui.SetCursorPosY( 8 );
			ImGui.Button( FontAwesome.Play, new System.Numerics.Vector2( 0, 32 ) );
			ImGui.Button( FontAwesome.Pause, new System.Numerics.Vector2( 0, 32 ) );
			ImGui.Button( FontAwesome.ForwardStep, new System.Numerics.Vector2( 0, 32 ) );

			// Draw on right
			var right = ImGui.GetMainViewport().WorkSize.X;
			right -= 42f;
			ImGui.SetCursorPosX( right );
			ImGui.SetCursorPosY( 8 );

			if ( ImGui.Button( FontAwesome.MagnifyingGlass, new System.Numerics.Vector2( 0, 32 ) ) )
			{
				quickSwitcherVisible = !quickSwitcherVisible;
				quickSwitcherInput = "";
			}

			ImGui.PopStyleVar();
			ImGui.PopStyleColor();
		}

		ImGui.EndMainMenuBar();
	}

	bool quickSwitcherVisible = false;
	string quickSwitcherInput = "";

	private void DrawQuickSwitcher()
	{
		var io = ImGui.GetIO();
		var windowFlags = ImGuiWindowFlags.NoDecoration |
			ImGuiWindowFlags.AlwaysAutoResize |
			ImGuiWindowFlags.NoSavedSettings |
			ImGuiWindowFlags.NoFocusOnAppearing |
			ImGuiWindowFlags.NoNav |
			ImGuiWindowFlags.NoInputs |
			ImGuiWindowFlags.NoTitleBar |
			ImGuiWindowFlags.NoMove;

		var windowSize = new System.Numerics.Vector2( 400, 200 );
		ImGui.SetNextWindowSize( windowSize );
		ImGui.SetNextWindowPos( (io.DisplaySize - windowSize) / 2.0f );

		var switcherItems = new List<(string, string)>();
		foreach ( var tab in tabs )
		{
			var editorMenuAttribute = tab.GetType().GetCustomAttribute<EditorMenuAttribute>();
			if ( editorMenuAttribute == null )
				continue;

			switcherItems.Add( ("Editor Menu:", editorMenuAttribute.Path) );
		}

		foreach ( var entity in Entity.All )
		{
			switcherItems.Add( ("Entity:", entity.Name) );
		}

		switcherItems.Add( ("Action:", "Play" ));
		switcherItems.Add( ("Action:", "Pause" ));
		switcherItems.Add( ("Action:", "Step" ));

		if ( ImGui.Begin( "Quick Switcher", windowFlags ) )
		{
			if ( !ImGui.IsAnyItemActive() && !ImGui.IsMouseClicked( 0 ) )
				ImGui.SetKeyboardFocusHere( 0 );

			ImGui.SetNextItemWidth( -1 );
			ImGui.InputText( "##quick_switcher_input", ref quickSwitcherInput, 128 );
			ImGui.BeginChild( "##quick_switcher_wrapper" );

			if ( ImGui.BeginTable( "##quick_switcher_table", 1, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
			{
				ImGui.TableSetupColumn( "Tab", ImGuiTableColumnFlags.WidthStretch, 1f );

				foreach ( var switcherItem in switcherItems )
				{
					if ( !string.IsNullOrEmpty( quickSwitcherInput ))
					{
						if ( !switcherItem.Item2.Contains( quickSwitcherInput, StringComparison.CurrentCultureIgnoreCase ) )
							continue;
					}

					ImGui.TableNextRow();
					ImGui.TableNextColumn();

					ImGui.PushStyleColor( ImGuiCol.Text, OneDark.Generic );
					ImGui.Text( switcherItem.Item1 );
					ImGui.PopStyleColor();
					ImGui.SameLine();
					ImGui.Text( switcherItem.Item2 );
				}

				ImGui.EndTable();
			}

			ImGui.EndChild();
			ImGui.End();
		}
	}

	public void Update()
	{
		if ( Input.Pressed( InputButton.ConsoleToggle ) )
			ShouldRender = !ShouldRender;

		Input.MouseMode = ShouldRender switch
		{
			true => Input.MouseModes.Unlocked,
			false => Input.MouseModes.Locked
		};

		Notify.Draw();

		if ( !ShouldRender )
		{
			DrawPerfOverlay();
			return;
		}

		Gizmos.Draw();
		EditorHelpers.DockSpaceOverViewport();
		DrawMenuBar();

		tabs.ForEach( tab =>
		{
			if ( tab.isVisible )
				tab.Draw();
		} );

		if ( quickSwitcherVisible )
			DrawQuickSwitcher();
	}
}

