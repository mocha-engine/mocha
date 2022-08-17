#include "CImgui.h"

#include "CRenderer.h"
#include "CWindow.h"
#include "FontAwesome.h"

#include <string>

ImFont* AddFont( ImGuiIO& io, std::string fontPath, float fontSize )
{
	ImFont* font = io.Fonts->AddFontFromFileTTF( fontPath.c_str(), fontSize );

	ImFontConfig* fontConfig = new ImFontConfig();
	fontConfig->MergeMode = 1;
	fontConfig->GlyphMinAdvanceX = fontSize + 8.0f;

	static const ImWchar ranges[] = { ICON_MIN_FA, ICON_MAX_FA, 0 };

	io.Fonts->AddFontFromFileTTF( "..\\content\\fonts\\fa-solid-900.ttf", fontSize, fontConfig, ranges );
	io.Fonts->AddFontFromFileTTF( "..\\content\\fonts\\fa-regular-400.ttf", fontSize, fontConfig, ranges );

	return font;
}

CImgui::CImgui( CWindow* window, CRenderer* renderer )
{
	ImGui::CreateContext();

	auto& colors = ImGui::GetStyle().Colors;
	colors[( int )ImGuiCol_Text] = ImVec4( 1.00f, 1.00f, 1.00f, 1.00f );
	colors[( int )ImGuiCol_TextDisabled] = ImVec4( 0.50f, 0.50f, 0.50f, 1.00f );
	colors[( int )ImGuiCol_WindowBg] = ImVec4( 0.17f, 0.17f, 0.18f, 1.00f );
	colors[( int )ImGuiCol_ChildBg] = ImVec4( 0.10f, 0.11f, 0.11f, 1.00f );
	colors[( int )ImGuiCol_PopupBg] = ImVec4( 0.24f, 0.24f, 0.25f, 1.00f );
	colors[( int )ImGuiCol_Border] = ImVec4( 0.00f, 0.00f, 0.00f, 0.5f );
	colors[( int )ImGuiCol_BorderShadow] = ImVec4( 0.00f, 0.00f, 0.00f, 0.24f );
	colors[( int )ImGuiCol_FrameBg] = ImVec4( 0.10f, 0.11f, 0.11f, 1.00f );
	colors[( int )ImGuiCol_FrameBgHovered] = ImVec4( 0.19f, 0.19f, 0.19f, 0.54f );
	colors[( int )ImGuiCol_FrameBgActive] = ImVec4( 0.20f, 0.22f, 0.23f, 1.00f );
	colors[( int )ImGuiCol_TitleBg] = ImVec4( 0.0f, 0.0f, 0.0f, 1.00f );
	colors[( int )ImGuiCol_TitleBgActive] = ImVec4( 0.00f, 0.00f, 0.00f, 1.00f );
	colors[( int )ImGuiCol_TitleBgCollapsed] = ImVec4( 0.00f, 0.00f, 0.00f, 1.00f );
	colors[( int )ImGuiCol_MenuBarBg] = ImVec4( 0.14f, 0.14f, 0.14f, 1.00f );
	colors[( int )ImGuiCol_ScrollbarBg] = ImVec4( 0.05f, 0.05f, 0.05f, 0.54f );
	colors[( int )ImGuiCol_ScrollbarGrab] = ImVec4( 0.34f, 0.34f, 0.34f, 0.54f );
	colors[( int )ImGuiCol_ScrollbarGrabHovered] = ImVec4( 0.40f, 0.40f, 0.40f, 0.54f );
	colors[( int )ImGuiCol_ScrollbarGrabActive] = ImVec4( 0.56f, 0.56f, 0.56f, 0.54f );
	colors[( int )ImGuiCol_CheckMark] = ImVec4( 0.33f, 0.67f, 0.86f, 1.00f );
	colors[( int )ImGuiCol_SliderGrab] = ImVec4( 0.34f, 0.34f, 0.34f, 0.54f );
	colors[( int )ImGuiCol_SliderGrabActive] = ImVec4( 0.56f, 0.56f, 0.56f, 0.54f );
	colors[( int )ImGuiCol_Button] = ImVec4( 0.24f, 0.24f, 0.25f, 1.00f );
	colors[( int )ImGuiCol_ButtonHovered] = ImVec4( 0.19f, 0.19f, 0.19f, 0.54f );
	colors[( int )ImGuiCol_ButtonActive] = ImVec4( 0.20f, 0.22f, 0.23f, 1.00f );
	colors[( int )ImGuiCol_Header] = ImVec4( 0.00f, 0.00f, 0.00f, 0.52f );
	colors[( int )ImGuiCol_HeaderHovered] = ImVec4( 0.00f, 0.00f, 0.00f, 0.36f );
	colors[( int )ImGuiCol_HeaderActive] = ImVec4( 0.20f, 0.22f, 0.23f, 0.33f );
	colors[( int )ImGuiCol_Separator] = ImVec4( 0.0f, 0.0f, 0.0f, 1.0f );
	colors[( int )ImGuiCol_SeparatorHovered] = ImVec4( 0.44f, 0.44f, 0.44f, 0.29f );
	colors[( int )ImGuiCol_SeparatorActive] = ImVec4( 0.40f, 0.44f, 0.47f, 1.00f );
	colors[( int )ImGuiCol_ResizeGrip] = ImVec4( 0.28f, 0.28f, 0.28f, 0.29f );
	colors[( int )ImGuiCol_ResizeGripHovered] = ImVec4( 0.44f, 0.44f, 0.44f, 0.29f );
	colors[( int )ImGuiCol_ResizeGripActive] = ImVec4( 0.40f, 0.44f, 0.47f, 1.00f );
	colors[( int )ImGuiCol_Tab] = ImVec4( 0.08f, 0.08f, 0.09f, 1.00f );
	colors[( int )ImGuiCol_TabHovered] = ImVec4( 0.14f, 0.14f, 0.14f, 1.00f );
	colors[( int )ImGuiCol_TabActive] = ImVec4( 0.17f, 0.17f, 0.18f, 1.00f );
	colors[( int )ImGuiCol_TabUnfocused] = ImVec4( 0.08f, 0.08f, 0.09f, 1.00f );
	colors[( int )ImGuiCol_TabUnfocusedActive] = ImVec4( 0.14f, 0.14f, 0.14f, 1.00f );
	colors[( int )ImGuiCol_PlotLines] = ImVec4( 0.33f, 0.67f, 0.86f, 1.00f );
	colors[( int )ImGuiCol_PlotLinesHovered] = ImVec4( 0.33f, 0.67f, 0.86f, 1.00f );
	colors[( int )ImGuiCol_PlotHistogram] = ImVec4( 0.33f, 0.67f, 0.86f, 1.00f );
	colors[( int )ImGuiCol_PlotHistogramHovered] = ImVec4( 0.33f, 0.67f, 0.86f, 1.00f );
	colors[( int )ImGuiCol_TableHeaderBg] = ImVec4( 0.00f, 0.00f, 0.00f, 0.52f );
	colors[( int )ImGuiCol_TableBorderStrong] = ImVec4( 0.00f, 0.00f, 0.00f, 0.52f );
	colors[( int )ImGuiCol_TableBorderLight] = ImVec4( 0.28f, 0.28f, 0.28f, 0.29f );
	colors[( int )ImGuiCol_TableRowBg] = ImVec4( 0.00f, 0.00f, 0.00f, 0.00f );
	colors[( int )ImGuiCol_TableRowBgAlt] = ImVec4( 1.00f, 1.00f, 1.00f, 0.06f );
	colors[( int )ImGuiCol_TextSelectedBg] = ImVec4( 0.20f, 0.22f, 0.23f, 1.00f );
	colors[( int )ImGuiCol_DragDropTarget] = ImVec4( 0.33f, 0.67f, 0.86f, 1.00f );
	colors[( int )ImGuiCol_NavHighlight] = ImVec4( 0.33f, 0.67f, 0.86f, 1.00f );
	colors[( int )ImGuiCol_NavWindowingHighlight] = ImVec4( 0.33f, 0.67f, 0.86f, 1.00f );
	colors[( int )ImGuiCol_NavWindowingDimBg] = ImVec4( 0.33f, 0.67f, 0.86f, 1.00f );
	colors[( int )ImGuiCol_ModalWindowDimBg] = ImVec4( 0.33f, 0.67f, 0.86f, 1.00f );

	auto& style = ImGui::GetStyle();
	style.WindowPadding = ImVec2( 8.00f, 8.00f );
	style.FramePadding = ImVec2( 12.00f, 6.00f );
	style.CellPadding = ImVec2( 4.00f, 4.00f );
	style.ItemSpacing = ImVec2( 4.00f, 4.00f );
	style.ItemInnerSpacing = ImVec2( 2.00f, 2.00f );
	style.TouchExtraPadding = ImVec2( 0.00f, 0.00f );
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
	style.WindowTitleAlign = ImVec2( 0.5f, 0.5f );
	style.WindowMenuButtonPosition = ImGuiDir_None;

	auto& io = ImGui::GetIO();
	io.ConfigFlags |= ImGuiConfigFlags_DockingEnable;
	io.ConfigDockingWithShift = true;

	mSansSerifFont = AddFont( io, "..\\content\\fonts\\Inter-Regular.ttf", 14.0f );
	mBoldFont = AddFont( io, "..\\content\\fonts\\Inter-Bold.ttf", 14.0f );
	mSubheadingFont = AddFont( io, "..\\content\\fonts\\Inter-Medium.ttf", 20.0f );
	mHeadingFont = AddFont( io, "..\\content\\fonts\\Inter-Bold.ttf", 24.0f );
	mMonospaceFont = io.Fonts->AddFontDefault();

	ImGui_ImplSDL2_InitForD3D( window->GetWindowPointer() );
	ImGui_ImplDX12_Init( renderer->GetDevice(), 1, DXGI_FORMAT_R8G8B8A8_UNORM, renderer->GetSRVHeap(),
	    renderer->GetSRVHeap()->GetCPUDescriptorHandleForHeapStart(),
	    renderer->GetSRVHeap()->GetGPUDescriptorHandleForHeapStart() );

	mWindow = window;
	mRenderer = renderer;
}

CImgui::~CImgui()
{
	ImGui_ImplDX12_Shutdown();
	ImGui_ImplSDL2_Shutdown();
	ImGui::DestroyContext();
}

void CImgui::NewFrame()
{
	ImGui_ImplDX12_NewFrame();
	ImGui_ImplSDL2_NewFrame( mWindow->GetWindowPointer() );
	ImGui::NewFrame();

	ImGui::DockSpaceOverViewport(
	    ImGui::GetMainViewport(), ImGuiDockNodeFlags_PassthruCentralNode | ImGuiDockNodeFlags_AutoHideTabBar );
}

void CImgui::Render( ID3D12GraphicsCommandList* commandList )
{
	ImGui::Render();
	ImGui_ImplDX12_RenderDrawData( ImGui::GetDrawData(), commandList );
}

void CImgui::Resize( Uint2 newSize )
{
	auto& io = ImGui::GetIO();
	io.DisplaySize = ImVec2( newSize.x, newSize.y );
}
