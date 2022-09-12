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
	private Renderer.Texture DefaultFontTexture { get; set; }

	private List<BaseEditorWindow> windows = new();

	// TODO: I don't like this
	internal RendererInstance Renderer { get; }

	public Editor( RendererInstance renderer )
	{
		Instance = this;
		Renderer = renderer;

		Init();
		SetTheme();

		windows.AddRange( Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where( x => typeof( BaseEditorWindow ).IsAssignableFrom( x ) )
			.Where( x => x != typeof( BaseEditorWindow ) )
			.Select( x => Activator.CreateInstance( x ) )
			.OfType<BaseEditorWindow>()
		);

		Logo = TextureBuilder.UITexture.FromPath( "logo.mtex" ).Build();
	}

	private static void AddIconFont( ImGuiIOPtr io, float fontSize )
	{
		unsafe
		{
			var iconConfig = ImGuiNative.ImFontConfig_ImFontConfig();
			iconConfig->MergeMode = 1;
			iconConfig->GlyphMinAdvanceX = fontSize * 2.0f;

			var iconRanges = new ushort[] { FontAwesome.IconMin, FontAwesome.IconMax, 0 };

			fixed ( ushort* rangePtr = iconRanges )
			{
				io.Fonts.AddFontFromFileTTF( "content/fonts/fa-solid-900.ttf", fontSize, iconConfig, (IntPtr)rangePtr );
				io.Fonts.AddFontFromFileTTF( "content/fonts/fa-regular-400.ttf", fontSize, iconConfig, (IntPtr)rangePtr );
			}
		}
	}

	public static Mocha.Renderer.Texture GenerateFontTexture()
	{
		var io = ImGui.GetIO();

		io.Fonts.Clear();

		SansSerifFont = io.Fonts.AddFontFromFileTTF( "content/fonts/Inter-Regular.ttf", 14f );
		AddIconFont( io, 12f );

		BoldFont = io.Fonts.AddFontFromFileTTF( "content/fonts/Inter-Bold.ttf", 14f );
		AddIconFont( io, 12f );

		SubheadingFont = io.Fonts.AddFontFromFileTTF( "content/fonts/Inter-Medium.ttf", 20f );
		AddIconFont( io, 16f );

		HeadingFont = io.Fonts.AddFontFromFileTTF( "content/fonts/Inter-Bold.ttf", 24f );
		AddIconFont( io, 20f );

		MonospaceFont = io.Fonts.AddFontDefault();

		io.Fonts.Build();
		io.Fonts.GetTexDataAsRGBA32( out IntPtr pixels, out var width, out var height, out var bpp );

		int size = width * height * bpp;
		byte[] data = new byte[size];
		Marshal.Copy( pixels, data, 0, size );

		return TextureBuilder.UITexture.FromData( data, (uint)width, (uint)height ).WithName( "ImGUI Font Texture" ).Build();
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

		io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable | ImGuiConfigFlags.IsSRGB | ImGuiConfigFlags.ViewportsEnable;
		io.ConfigDockingWithShift = true;

		ImGui.LoadIniSettingsFromDisk( ImGui.GetIO().IniFilename ); // https://github.com/mellinoe/veldrid/issues/410

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

		ImGui.PushStyleVar( ImGuiStyleVar.WindowBorderSize, 0 );
		ImGui.SetNextWindowPos( windowPos, ImGuiCond.Always, windowPivot );
		ImGui.SetNextWindowBgAlpha( 0.5f );
		ImGui.SetNextWindowSize( new System.Numerics.Vector2( 125, 0 ) );

		if ( ImGui.Begin( $"##overlay", windowFlags ) )
		{
			string total = GC.GetTotalMemory( false ).ToSize( MathX.SizeUnits.MB );

			ImGui.PushFont( Editor.SubheadingFont );
			ImGui.Text( $"{io.Framerate.CeilToInt()}fps" );
			ImGui.PopFont();

			ImGui.PushFont( Editor.BoldFont );
			ImGui.Text( $"{total} total" );
			ImGui.PopFont();

			ImGui.Text( "F1 for editor" );

			ImGui.End();
		}

		ImGui.PopStyleVar( 1 );
	}

	private void DrawMenuBar()
	{
		ImGuiX.MenusSubmittedThisFrame.Clear();

		ImGui.PushStyleColor( ImGuiCol.MenuBarBg, MathX.GetColor( "#000000" ) );
		ImGui.PushStyleVar( ImGuiStyleVar.FramePadding, new System.Numerics.Vector2( 0, 16 ) );
		ImGui.BeginMainMenuBar();

		ImGui.Dummy( new( 4, 0 ) );

		ImGui.SetCursorPosY( 8 );
		ImGuiX.Image( Logo, new Vector2( 32, 32 ) );
		ImGui.SetCursorPosY( 0 );
		ImGui.Dummy( new( 4, 0 ) );

		if ( ImGuiX.BeginMenu( $"Tools" ) )
		{
			ImGuiX.MenuItem( FontAwesome.Image, "Texture Tool" );
			ImGuiX.MenuItem( FontAwesome.FaceGrinStars, "Material Tool" );
			ImGuiX.MenuItem( FontAwesome.Cubes, "Model Tool" );
			ImGuiX.MenuItem( FontAwesome.Glasses, "Shader Tool" );
			ImGuiX.EndMenu();
		}

		foreach ( var window in windows )
		{
			var displayInfo = DisplayInfo.For( window );

			if ( ImGuiX.BeginMenu( displayInfo.Category ) )
			{
				var enabled = window.isVisible;
				bool active = ImGuiX.MenuItem( displayInfo.TextIcon, displayInfo.Name, enabled );

				if ( active )
					window.isVisible = !window.isVisible;

				ImGuiX.EndMenu();
			}
		}

		ImGui.PopStyleVar();

		//
		// Buttons
		//
		{
			ImGui.PushStyleVar( ImGuiStyleVar.FramePadding, new System.Numerics.Vector2( 4, 0 ) );
			ImGui.PushStyleColor( ImGuiCol.Button, System.Numerics.Vector4.Zero );

			// Draw play, pause in center
			var center = ImGui.GetMainViewport().WorkSize.X / 2.0f;
			center -= 40f; // Approx.
			ImGui.SetCursorPosX( center );
			ImGui.SetCursorPosY( 8 );

			void DrawButtonUnderline()
			{
				var drawList = ImGui.GetWindowDrawList();
				var buttonCol = ImGui.GetColorU32( Colors.Blue );

				var p0 = ImGui.GetCursorPos() + new System.Numerics.Vector2( 0, 32 );
				var p1 = p0 + new System.Numerics.Vector2( 32, 4 );
				drawList.AddRectFilled( p0, p1, buttonCol, 4f );
			}

			//
			// Play button
			//
			{
				if ( World.Current.State == World.States.Playing )
				{
					DrawButtonUnderline();
				}

				if ( ImGui.Button( FontAwesome.Play, new System.Numerics.Vector2( 0, 32 ) ) )
					World.Current.State = World.States.Playing;
			}

			//
			// Pause button
			//
			{
				if ( World.Current.State == World.States.Paused )
				{
					DrawButtonUnderline();
				}

				if ( ImGui.Button( FontAwesome.Pause, new System.Numerics.Vector2( 0, 32 ) ) )
					World.Current.State = World.States.Paused;
			}

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
		ImGui.PopStyleColor();
	}

	bool quickSwitcherVisible = false;
	string quickSwitcherInput = "";
	int selectedQuickSwitcherItem = 0;
	bool quickSwitcherUsingKeyboard = false;
	Vector2 lastMousePos = default;

	// TODO: Refactor
	private void DrawQuickSwitcher()
	{
		if ( Input.Pressed( InputButton.QuickSwitcher ) )
			quickSwitcherVisible = !quickSwitcherVisible;

		if ( !quickSwitcherVisible )
			return;

		var io = ImGui.GetIO();
		var windowFlags = ImGuiWindowFlags.NoDecoration |
			ImGuiWindowFlags.AlwaysAutoResize |
			ImGuiWindowFlags.NoSavedSettings |
			ImGuiWindowFlags.NoFocusOnAppearing |
			ImGuiWindowFlags.NoNav |
			ImGuiWindowFlags.NoInputs |
			ImGuiWindowFlags.NoTitleBar |
			ImGuiWindowFlags.NoMove;

		var windowSize = new System.Numerics.Vector2( 600, 400 );
		var center = (io.DisplaySize - windowSize) / 2.0f;
		ImGui.SetNextWindowSize( windowSize );
		ImGui.SetNextWindowPos( new System.Numerics.Vector2( center.X, 100 ) );

		var switcherItems = new List<(string, string)>();

		foreach ( var entity in Entity.All )
		{
			switcherItems.Add( ("Entity", entity.Name) );
		}

		foreach ( var asset in BrowserWindow.Instance.fileSystemCache )
		{
			switcherItems.Add( ("Asset", asset.Item2.NormalizePath()) );
		}

		if ( ImGui.Begin( "Quick Switcher", windowFlags ) )
		{
			if ( !ImGui.IsAnyItemActive() && !ImGui.IsMouseClicked( 0 ) )
				ImGui.SetKeyboardFocusHere( 0 );

			ImGui.SetNextItemWidth( -1 );
			if ( ImGui.InputText( "##quick_switcher_input", ref quickSwitcherInput, 128 ) )
				quickSwitcherUsingKeyboard = true;

			ImGui.BeginListBox( "##quick_switcher_wrapper", new System.Numerics.Vector2( -1, -1 ) );

			var selectedItem = ("", "");

			if ( ImGui.BeginTable( "##quick_switcher_table", 1, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
			{
				ImGui.TableSetupColumn( "Tab", ImGuiTableColumnFlags.WidthStretch, 1f );

				int index = 0;

				for ( int i = 0; i < switcherItems.Count; i++ )
				{
					(string, string) switcherItem = switcherItems[i];

					if ( !string.IsNullOrEmpty( quickSwitcherInput ) )
					{
						bool foundAll = true;
						var inputs = quickSwitcherInput.Split( " " );

						foreach ( var input in inputs )
							if ( !switcherItem.Item2.Contains( input, StringComparison.CurrentCultureIgnoreCase ) )
								foundAll = false;

						if ( !foundAll )
							continue;
					}

					var startPos = ImGui.GetCursorPos();

					if ( quickSwitcherUsingKeyboard )
					{
						if ( index == selectedQuickSwitcherItem )
						{
							var windowPos = ImGui.GetWindowPos();
							var drawList = ImGui.GetWindowDrawList();
							var scrollPos = new System.Numerics.Vector2( 0, ImGui.GetScrollY() );

							selectedItem = switcherItem;

							drawList.AddRectFilled(
								windowPos + startPos + new System.Numerics.Vector2( 0, 0 ) - scrollPos,
								windowPos + startPos + new System.Numerics.Vector2( 1000, 24 ) - scrollPos,
								ImGui.GetColorU32( Colors.Blue * 0.75f ) );

							ImGui.SetScrollHereY();
						}
					}

					ImGui.TableNextRow();
					ImGui.TableNextColumn();

					ImGui.PushStyleColor( ImGuiCol.Text, Colors.LightText );
					ImGui.Text( $"{switcherItem.Item1}:".Pad() );
					ImGui.PopStyleColor();
					ImGui.SameLine();
					ImGui.Text( switcherItem.Item2 );

					if ( !quickSwitcherUsingKeyboard )
					{
						if ( ImGui.IsItemHovered() )
						{
							var windowPos = ImGui.GetWindowPos();
							var drawList = ImGui.GetWindowDrawList();
							var scrollPos = new System.Numerics.Vector2( 0, ImGui.GetScrollY() );

							selectedItem = switcherItem;

							drawList.AddRectFilled(
								windowPos + startPos + new System.Numerics.Vector2( 0, 0 ) - scrollPos,
								windowPos + startPos + new System.Numerics.Vector2( 1000, 24 ) - scrollPos,
								ImGui.GetColorU32( Colors.Blue * 0.75f ) );
						}
					}

					index++;
				}

				ImGui.EndTable();
			}

			if ( ImGui.IsKeyPressed( ImGuiKey.DownArrow ) )
				selectedQuickSwitcherItem++;

			if ( ImGui.IsKeyPressed( ImGuiKey.UpArrow ) )
				selectedQuickSwitcherItem--;

			if ( ImGui.IsKeyPressed( ImGuiKey.PageDown ) )
				selectedQuickSwitcherItem += 10;

			if ( ImGui.IsKeyPressed( ImGuiKey.PageUp ) )
				selectedQuickSwitcherItem -= 10;

			if ( selectedQuickSwitcherItem < 0 )
				selectedQuickSwitcherItem = 0;

			var mousePos = new Vector2( ImGui.GetMousePos() );
			var mouseDelta = (mousePos - lastMousePos).Length;

			if ( mouseDelta > 1.0f )
				quickSwitcherUsingKeyboard = false;

			lastMousePos = mousePos;

			if ( ImGui.IsKeyPressed( ImGuiKey.Enter ) || ImGui.IsMouseClicked( ImGuiMouseButton.Left ) )
			{
				switch ( selectedItem.Item1 )
				{
					case "Asset":
						ImGui.SetWindowFocus( "Browser" );
						BrowserWindow.Instance.SelectItem( selectedItem.Item2 );
						ImGui.SetWindowFocus( "Inspector" );
						break;
					case "Menu":
						ImGui.SetWindowFocus( selectedItem.Item2.Split( '/' )[^1] );
						break;
					case "Entity":
						OutlinerTab.Instance.SelectItem( selectedItem.Item2 );
						break;
					default:
						break;
				}

				selectedQuickSwitcherItem = 0;
				quickSwitcherInput = "";
				quickSwitcherVisible = false;
			}

			ImGui.EndListBox();
			ImGui.End();
		}
	}

	public void Update()
	{
		if ( Input.Pressed( InputButton.ConsoleToggle ) )
			ShouldRender = !ShouldRender;

		if ( Input.Pressed( InputButton.SwitchMode ) )
		{
			if ( World.Current.State == World.States.Playing )
				World.Current.State = World.States.Paused;
			else if ( World.Current.State == World.States.Paused )
				World.Current.State = World.States.Playing;
		}

		Input.MouseMode = ShouldRender switch
		{
			true => Input.MouseModes.Unlocked,
			false => Input.MouseModes.Locked
		};

		Notify.Draw();

		if ( !ShouldRender )
		{
			DrawPerfOverlay();
			SceneWorld.Current.Camera.UpdateAspect( Window.Current.Size );
			return;
		}

		ImGuiX.DockSpaceOverViewport();

		DrawMenuBar();
		DrawQuickSwitcher();

		windows.ForEach( window =>
		{
			if ( window.isVisible )
				window.Draw();
		} );
	}
}

