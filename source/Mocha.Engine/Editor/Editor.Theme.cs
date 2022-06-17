﻿using ImGuiNET;

namespace Mocha.Engine;

partial class Editor
{
	public unsafe void SetTheme()
	{
		var colors = ImGui.GetStyle().Colors;
		colors[(int)ImGuiCol.Text] = new( 1.00f, 1.00f, 1.00f, 0.78f );
		colors[(int)ImGuiCol.TextDisabled] = new( 1.00f, 1.00f, 1.00f, 0.28f );
		colors[(int)ImGuiCol.WindowBg] = new( 0.21f, 0.22f, 0.26f, 1.00f );
		colors[(int)ImGuiCol.ChildBg] = new( 0.18f, 0.20f, 0.23f, 0.58f );
		colors[(int)ImGuiCol.PopupBg] = new( 0.18f, 0.20f, 0.23f, 0.90f );
		colors[(int)ImGuiCol.Border] = new( 0.14f, 0.15f, 0.18f, 0.60f );
		colors[(int)ImGuiCol.BorderShadow] = new( 0.18f, 0.20f, 0.23f, 0.00f );
		colors[(int)ImGuiCol.FrameBg] = new( 0.18f, 0.20f, 0.23f, 1.00f );
		colors[(int)ImGuiCol.FrameBgHovered] = new( 0.24f, 0.26f, 0.30f, 0.78f );
		colors[(int)ImGuiCol.FrameBgActive] = new( 0.24f, 0.26f, 0.30f, 1.00f );
		colors[(int)ImGuiCol.TitleBg] = new( 0.11f, 0.11f, 0.13f, 1.00f );
		colors[(int)ImGuiCol.TitleBgActive] = new( 0.32f, 0.35f, 0.40f, 1.00f );
		colors[(int)ImGuiCol.TitleBgCollapsed] = new( 0.18f, 0.20f, 0.23f, 0.75f );
		colors[(int)ImGuiCol.MenuBarBg] = new( 0.18f, 0.20f, 0.23f, 0.47f );
		colors[(int)ImGuiCol.ScrollbarBg] = new( 0.18f, 0.20f, 0.23f, 1.00f );
		colors[(int)ImGuiCol.ScrollbarGrab] = new( 0.11f, 0.11f, 0.13f, 1.00f );
		colors[(int)ImGuiCol.ScrollbarGrabHovered] = new( 0.24f, 0.26f, 0.30f, 0.78f );
		colors[(int)ImGuiCol.ScrollbarGrabActive] = new( 0.24f, 0.26f, 0.30f, 1.00f );
		colors[(int)ImGuiCol.CheckMark] = new( 0.32f, 0.35f, 0.40f, 1.00f );
		colors[(int)ImGuiCol.SliderGrab] = new( 0.26f, 0.28f, 0.33f, 1.00f );
		colors[(int)ImGuiCol.SliderGrabActive] = new( 0.32f, 0.35f, 0.40f, 1.00f );
		colors[(int)ImGuiCol.Button] = new( 0.34f, 0.37f, 0.43f, 1.00f );
		colors[(int)ImGuiCol.ButtonHovered] = new( 0.24f, 0.26f, 0.30f, 1.00f );
		colors[(int)ImGuiCol.ButtonActive] = new( 0.32f, 0.35f, 0.40f, 1.00f );
		colors[(int)ImGuiCol.Header] = new( 0.24f, 0.26f, 0.30f, 0.76f );
		colors[(int)ImGuiCol.HeaderHovered] = new( 0.24f, 0.26f, 0.30f, 0.86f );
		colors[(int)ImGuiCol.HeaderActive] = new( 0.32f, 0.35f, 0.40f, 1.00f );
		colors[(int)ImGuiCol.Separator] = new( 0.28f, 0.28f, 0.28f, 0.29f );
		colors[(int)ImGuiCol.SeparatorHovered] = new( 0.44f, 0.44f, 0.44f, 0.29f );
		colors[(int)ImGuiCol.SeparatorActive] = new( 0.40f, 0.44f, 0.47f, 1.00f );
		colors[(int)ImGuiCol.ResizeGrip] = new( 0.47f, 0.77f, 0.83f, 0.04f );
		colors[(int)ImGuiCol.ResizeGripHovered] = new( 0.24f, 0.26f, 0.30f, 0.78f );
		colors[(int)ImGuiCol.ResizeGripActive] = new( 0.24f, 0.26f, 0.30f, 1.00f );
		colors[(int)ImGuiCol.Tab] = new( 0.18f, 0.20f, 0.23f, 0.40f );
		colors[(int)ImGuiCol.TabHovered] = new( 0.32f, 0.35f, 0.40f, 1.00f );
		colors[(int)ImGuiCol.TabActive] = new( 0.24f, 0.26f, 0.30f, 1.00f );
		colors[(int)ImGuiCol.TabUnfocused] = new( 0.18f, 0.20f, 0.23f, 0.40f );
		colors[(int)ImGuiCol.TabUnfocusedActive] = new( 0.18f, 0.20f, 0.23f, 0.70f );
		colors[(int)ImGuiCol.DockingPreview] = new( 0.32f, 0.35f, 0.40f, 0.30f );
		colors[(int)ImGuiCol.DockingEmptyBg] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.PlotLines] = new( 1.00f, 1.00f, 1.00f, 0.63f );
		colors[(int)ImGuiCol.PlotLinesHovered] = new( 0.24f, 0.26f, 0.30f, 1.00f );
		colors[(int)ImGuiCol.PlotHistogram] = new( 1.00f, 1.00f, 1.00f, 0.63f );
		colors[(int)ImGuiCol.PlotHistogramHovered] = new( 0.24f, 0.26f, 0.30f, 1.00f );
		colors[(int)ImGuiCol.TableHeaderBg] = new( 0.00f, 0.00f, 0.00f, 0.52f );
		colors[(int)ImGuiCol.TableBorderStrong] = new( 0.00f, 0.00f, 0.00f, 0.52f );
		colors[(int)ImGuiCol.TableBorderLight] = new( 0.28f, 0.28f, 0.28f, 0.29f );
		colors[(int)ImGuiCol.TableRowBg] = new( 0.00f, 0.00f, 0.00f, 0.00f );
		colors[(int)ImGuiCol.TableRowBgAlt] = new( 1.00f, 1.00f, 1.00f, 0.06f );
		colors[(int)ImGuiCol.TextSelectedBg] = new( 0.24f, 0.26f, 0.30f, 0.43f );
		colors[(int)ImGuiCol.DragDropTarget] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.NavHighlight] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.NavWindowingHighlight] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.NavWindowingDimBg] = new( 0.33f, 0.67f, 0.86f, 1.00f );
		colors[(int)ImGuiCol.ModalWindowDimBg] = new( 0.18f, 0.20f, 0.23f, 0.73f );

		var style = ImGui.GetStyle();
		style.WindowPadding = new( 8.00f, 8.00f );
		style.FramePadding = new( 8.00f, 4.00f );
		style.CellPadding = new( 6.00f, 6.00f );
		style.ItemSpacing = new( 4.00f, 4.00f );
		style.ItemInnerSpacing = new( 2.00f, 2.00f );
		style.TouchExtraPadding = new( 0.00f, 0.00f );
		style.IndentSpacing = 25;
		style.ScrollbarSize = 10;
		style.GrabMinSize = 10;
		style.WindowBorderSize = 1;
		style.ChildBorderSize = 1;
		style.PopupBorderSize = 1;
		style.FrameBorderSize = 1;
		style.TabBorderSize = 1;
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
	}
}
