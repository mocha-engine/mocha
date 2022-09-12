using ImGuiNET;

namespace Mocha.Engine;

partial class Editor
{
	public unsafe void SetTheme()
	{
		var colors = ImGui.GetStyle().Colors;
		colors[(int)ImGuiCol.Text] = new( 1.00f, 1.00f, 1.00f, 1.00f );
		colors[(int)ImGuiCol.TextDisabled] = new( 0.50f, 0.50f, 0.50f, 1.00f );
		colors[(int)ImGuiCol.WindowBg] = new( 0.17f, 0.17f, 0.18f, 1.00f );
		colors[(int)ImGuiCol.ChildBg] = new( 0.10f, 0.11f, 0.11f, 1.00f );
		colors[(int)ImGuiCol.PopupBg] = new( 0.24f, 0.24f, 0.25f, 1.00f );
		colors[(int)ImGuiCol.Border] = new( 0.00f, 0.00f, 0.00f, 0.5f );
		colors[(int)ImGuiCol.BorderShadow] = new( 0.00f, 0.00f, 0.00f, 0.24f );
		colors[(int)ImGuiCol.FrameBg] = new( 0.10f, 0.11f, 0.11f, 1.00f );
		colors[(int)ImGuiCol.FrameBgHovered] = new( 0.19f, 0.19f, 0.19f, 0.54f );
		colors[(int)ImGuiCol.FrameBgActive] = new( 0.20f, 0.22f, 0.23f, 1.00f );
		colors[(int)ImGuiCol.TitleBg] = new( 0.0f, 0.0f, 0.0f, 1.00f );
		colors[(int)ImGuiCol.TitleBgActive] = new( 0.00f, 0.00f, 0.00f, 1.00f );
		colors[(int)ImGuiCol.TitleBgCollapsed] = new( 0.00f, 0.00f, 0.00f, 1.00f );
		colors[(int)ImGuiCol.MenuBarBg] = new( 0.14f, 0.14f, 0.14f, 1.00f );
		colors[(int)ImGuiCol.ScrollbarBg] = new( 0.05f, 0.05f, 0.05f, 0.54f );
		colors[(int)ImGuiCol.ScrollbarGrab] = new( 0.34f, 0.34f, 0.34f, 0.54f );
		colors[(int)ImGuiCol.ScrollbarGrabHovered] = new( 0.40f, 0.40f, 0.40f, 0.54f );
		colors[(int)ImGuiCol.ScrollbarGrabActive] = new( 0.56f, 0.56f, 0.56f, 0.54f );
		colors[(int)ImGuiCol.CheckMark] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.SliderGrab] = new( 0.34f, 0.34f, 0.34f, 0.54f );
		colors[(int)ImGuiCol.SliderGrabActive] = new( 0.56f, 0.56f, 0.56f, 0.54f );
		colors[(int)ImGuiCol.Button] = new( 0.24f, 0.24f, 0.25f, 1.00f );
		colors[(int)ImGuiCol.ButtonHovered] = new( 0.19f, 0.19f, 0.19f, 0.54f );
		colors[(int)ImGuiCol.ButtonActive] = new( 0.20f, 0.22f, 0.23f, 1.00f );
		colors[(int)ImGuiCol.Header] = new( 0.00f, 0.00f, 0.00f, 0.52f );
		colors[(int)ImGuiCol.HeaderHovered] = new( 0.00f, 0.00f, 0.00f, 0.36f );
		colors[(int)ImGuiCol.HeaderActive] = new( 0.20f, 0.22f, 0.23f, 0.33f );
		colors[(int)ImGuiCol.Separator] = new( 0.0f, 0.0f, 0.0f, 1.0f );
		colors[(int)ImGuiCol.SeparatorHovered] = new( 0.44f, 0.44f, 0.44f, 0.29f );
		colors[(int)ImGuiCol.SeparatorActive] = new( 0.40f, 0.44f, 0.47f, 1.00f );
		colors[(int)ImGuiCol.ResizeGrip] = new( 0.28f, 0.28f, 0.28f, 0.29f );
		colors[(int)ImGuiCol.ResizeGripHovered] = new( 0.44f, 0.44f, 0.44f, 0.29f );
		colors[(int)ImGuiCol.ResizeGripActive] = new( 0.40f, 0.44f, 0.47f, 1.00f );
		colors[(int)ImGuiCol.Tab] = new( 0.08f, 0.08f, 0.09f, 1.00f );
		colors[(int)ImGuiCol.TabHovered] = new( 0.14f, 0.14f, 0.14f, 1.00f );
		colors[(int)ImGuiCol.TabActive] = new( 0.17f, 0.17f, 0.18f, 1.00f );
		colors[(int)ImGuiCol.TabUnfocused] = new( 0.08f, 0.08f, 0.09f, 1.00f );
		colors[(int)ImGuiCol.TabUnfocusedActive] = new( 0.14f, 0.14f, 0.14f, 1.00f );
		colors[(int)ImGuiCol.DockingPreview] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.DockingEmptyBg] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.PlotLines] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.PlotLinesHovered] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.PlotHistogram] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.PlotHistogramHovered] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.TableHeaderBg] = new( 0.00f, 0.00f, 0.00f, 0.52f );
		colors[(int)ImGuiCol.TableBorderStrong] = new( 0.00f, 0.00f, 0.00f, 0.52f );
		colors[(int)ImGuiCol.TableBorderLight] = new( 0.28f, 0.28f, 0.28f, 0.29f );
		colors[(int)ImGuiCol.TableRowBg] = new( 0.00f, 0.00f, 0.00f, 0.00f );
		colors[(int)ImGuiCol.TableRowBgAlt] = new( 1.00f, 1.00f, 1.00f, 0.06f );
		colors[(int)ImGuiCol.TextSelectedBg] = new( 0.20f, 0.22f, 0.23f, 1.00f );
		colors[(int)ImGuiCol.DragDropTarget] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.NavHighlight] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.NavWindowingHighlight] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.NavWindowingDimBg] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.ModalWindowDimBg] = new( 0.33f, 0.67f, 0.86f, 1.00f );

		var style = ImGui.GetStyle();
		style.WindowPadding = new( 8.00f, 8.00f );
		style.FramePadding = new( 12.00f, 6.00f );
		style.CellPadding = new( 4.00f, 4.00f );
		style.ItemSpacing = new( 4.00f, 4.00f );
		style.ItemInnerSpacing = new( 2.00f, 2.00f );
		style.TouchExtraPadding = new( 0.00f, 0.00f );
		style.IndentSpacing = 25;
		style.ScrollbarSize = 12;
		style.GrabMinSize = 12;
		style.WindowBorderSize = 1;
		style.ChildBorderSize = 0;
		style.PopupBorderSize = 0;
		style.FrameBorderSize = 0;
		style.TabBorderSize = 0;
		style.WindowRounding = 6;
		style.ChildRounding = 4;
		style.FrameRounding = 3;
		style.PopupRounding = 4;
		style.ScrollbarRounding = 9;
		style.GrabRounding = 3;
		style.LogSliderDeadzone = 4;
		style.TabRounding = 4;
		style.WindowTitleAlign = new( 0.5f, 0.5f );
		style.WindowMenuButtonPosition = ImGuiDir.None;
		style.AntiAliasedLinesUseTex = false;
	}
}
