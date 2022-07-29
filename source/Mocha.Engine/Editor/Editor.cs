using ImGuiNET;
using Mocha.Common;
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
		io.ConfigDockingWithShift = true;
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

		ImGui.PushStyleVar( ImGuiStyleVar.WindowBorderSize, 0 );
		ImGui.PushStyleVar( ImGuiStyleVar.WindowRounding, 0 );
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

		ImGui.PopStyleVar( 2 );
	}

	private void DrawMenuBar()
	{
		EditorHelpers.MenusSubmittedThisFrame.Clear();

		ImGui.PushStyleColor( ImGuiCol.MenuBarBg, MathX.GetColor( "#000000" ) );
		ImGui.PushStyleVar( ImGuiStyleVar.FramePadding, new System.Numerics.Vector2( 0, 16 ) );
		ImGui.BeginMainMenuBar();

		ImGui.Dummy( new( 4, 0 ) );

		ImGui.SetCursorPosY( 8 );
		EditorHelpers.Image( Logo, new Vector2( 32, 32 ) );
		ImGui.SetCursorPosY( 0 );

		ImGui.Dummy( new( 4, 0 ) );

		foreach ( var tab in tabs )
		{
			var editorMenuAttribute = tab.GetType().GetCustomAttribute<EditorMenuAttribute>();
			if ( editorMenuAttribute == null )
				continue;

			var splitPath = editorMenuAttribute.Path.Split( '/' );

			if ( EditorHelpers.BeginMenu( splitPath[0] ) )
			{
				for ( int i = 1; i < splitPath.Length; i++ )
				{
					string item = splitPath[i];
					var icon = splitPath[0][0].ToString();
					var name = item;
					bool active = EditorHelpers.MenuItem( icon, name );

					if ( i == splitPath.Length - 1 && active )
						tab.isVisible = !tab.isVisible;
				}

				EditorHelpers.EndMenu();
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
		ImGui.PopStyleColor();
	}

	bool quickSwitcherVisible = false;
	string quickSwitcherInput = "";
	int selectedQuickSwitcherItem = 0;

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
		foreach ( var tab in tabs )
		{
			var editorMenuAttribute = tab.GetType().GetCustomAttribute<EditorMenuAttribute>();
			if ( editorMenuAttribute == null )
				continue;

			switcherItems.Add( ("Menu", editorMenuAttribute.Path) );
		}

		foreach ( var entity in Entity.All )
		{
			switcherItems.Add( ("Entity", entity.Name) );
		}

		foreach ( var asset in AssetsTab.Instance.fileSystemCache )
		{
			switcherItems.Add( ("Asset", asset.Item2.NormalizePath()) );
		}

		if ( ImGui.Begin( "Quick Switcher", windowFlags ) )
		{
			if ( !ImGui.IsAnyItemActive() && !ImGui.IsMouseClicked( 0 ) )
				ImGui.SetKeyboardFocusHere( 0 );

			ImGui.SetNextItemWidth( -1 );
			ImGui.InputText( "##quick_switcher_input", ref quickSwitcherInput, 128 );
			ImGui.BeginChild( "##quick_switcher_wrapper" );

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

					if ( index == selectedQuickSwitcherItem )
					{
						var windowPos = ImGui.GetWindowPos();
						var drawList = ImGui.GetWindowDrawList();
						var scrollPos = new System.Numerics.Vector2( 0, ImGui.GetScrollY() );
						var startPos = ImGui.GetCursorPos();

						selectedItem = switcherItem;

						drawList.AddRectFilled(
							windowPos + startPos + new System.Numerics.Vector2( 0, 0 ) - scrollPos,
							windowPos + startPos + new System.Numerics.Vector2( 1000, 24 ) - scrollPos,
							ImGui.GetColorU32( OneDark.Info * 0.75f ) );

						if ( !ImGui.IsRectVisible( windowPos + startPos - scrollPos - new System.Numerics.Vector2( 0, 32 ) ) )
							ImGui.SetScrollHereY();
					}

					ImGui.TableNextRow();
					ImGui.TableNextColumn();

					ImGui.PushStyleColor( ImGuiCol.Text, OneDark.Generic );
					ImGui.Text( $"{switcherItem.Item1}:" );
					ImGui.PopStyleColor();
					ImGui.SameLine();
					ImGui.Text( switcherItem.Item2 );

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

			if ( ImGui.IsKeyPressed( ImGuiKey.Enter ) )
			{
				switch ( selectedItem.Item1 )
				{
					case "Asset":
						ImGui.SetWindowFocus( "Browser" );
						AssetsTab.Instance.SelectItem( selectedItem.Item2 );
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
			SceneWorld.Current.Camera.UpdateAspect( Window.Current.Size );
			return;
		}

		Gizmos.Draw();

		EditorHelpers.DockSpaceOverViewport();

		DrawMenuBar();
		DrawQuickSwitcher();

		tabs.ForEach( tab =>
		{
			if ( tab.isVisible )
				tab.Draw();
		} );
	}
}

