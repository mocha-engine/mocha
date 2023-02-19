#include "editormanager.h"

#include <Rendering/rendermanager.h>
#include <imgui.h>
#include <imgui_internal.h>
#include <implot.h>

void EditorManager::Startup() {}
void EditorManager::Shutdown() {}

void* EditorManager::GetContextPointer()
{
	auto ctx = ImGui::GetCurrentContext();
	return ( void* )ctx;
}

void EditorManager::TextBold( const char* text )
{
	ImGui::PushFont( Globals::m_renderContext->m_boldFont );
	ImGui::Text( "%s", text );
	ImGui::PopFont();
}

void EditorManager::TextSubheading( const char* text )
{
	ImGui::PushFont( Globals::m_renderContext->m_subheadingFont );
	ImGui::Text( "%s", text );
	ImGui::Dummy( ImVec2( 0, 2 ) );
	ImGui::PopFont();
}

void EditorManager::TextHeading( const char* text )
{
	ImGui::PushFont( Globals::m_renderContext->m_headingFont );
	ImGui::Text( "%s", text );
	ImGui::Dummy( ImVec2( 0, 2 ) );
	ImGui::PopFont();
}

void EditorManager::TextMonospace( const char* text )
{
	ImGui::PushFont( Globals::m_renderContext->m_monospaceFont );
	ImGui::Text( "%s", text );
	ImGui::PopFont();
}

void EditorManager::TextLight( const char* text )
{
	ImGui::PushStyleColor( ImGuiCol_Text, ImVec4( 1, 1, 1, 0.75f ) );
	ImGui::Text( "%s", text );
	ImGui::PopStyleColor();
}

const char* EditorManager::GetGPUName()
{
	return Globals::m_renderManager->GetGPUName();
}

char* EditorManager::InputText( const char* name, char* inputBuf, int inputLength )
{
	ImGui::InputText( name, inputBuf, inputLength, ImGuiInputTextFlags_EnterReturnsTrue );

	return inputBuf;
}

void EditorManager::RenderViewDropdown()
{
	if ( ImGui::BeginMenu( "Debug View" ) )
	{
		if ( ImGui::MenuItem( "None" ) )
			Globals::m_debugView = RenderDebugViews::NONE;

		if ( ImGui::MenuItem( "Diffuse" ) )
			Globals::m_debugView = RenderDebugViews::DIFFUSE;

		if ( ImGui::MenuItem( "Normal" ) )
			Globals::m_debugView = RenderDebugViews::NORMAL;

		if ( ImGui::MenuItem( "Ambient Occlusion" ) )
			Globals::m_debugView = RenderDebugViews::AMBIENTOCCLUSION;

		if ( ImGui::MenuItem( "Metalness" ) )
			Globals::m_debugView = RenderDebugViews::METALNESS;

		if ( ImGui::MenuItem( "Roughness" ) )
			Globals::m_debugView = RenderDebugViews::ROUGHNESS;

		if ( ImGui::MenuItem( "Other" ) )
			Globals::m_debugView = RenderDebugViews::OTHER;

		ImGui::EndMenu();
	}
}

void EditorManager::Image( Texture* texture, uint32_t textureWidth, uint32_t textureHeight, int x, int y )
{
	void* imguiTextureID;
	Globals::m_renderContext->GetImGuiTextureID( &texture->m_image, &imguiTextureID );

	// Calculate new UVs based on reported textureWidth, textureHeight vs texture->m_size
	// This is done because the C++ side isn't aware of any padding applied in order to get
	// the image to become POT
	float u = ( float )textureWidth / ( float )texture->m_size.x;
	float v = ( float )textureWidth / ( float )texture->m_size.y;

	ImGui::Image( imguiTextureID, { ( float )x, ( float )y }, { 0, 0 }, { u, v } );
}

bool EditorManager::BeginMainStatusBar()
{
	ImGuiViewportP* viewport = ( ImGuiViewportP* )( void* )ImGui::GetMainViewport();
	ImGuiWindowFlags window_flags = ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_MenuBar;
	float height = ImGui::GetFrameHeight();

	if ( ImGui::BeginViewportSideBar( "##MainStatusBar", viewport, ImGuiDir_Down, height, window_flags ) )
	{
		if ( ImGui::BeginMenuBar() )
		{
			return true;
		}
	}

	return false;
}

void EditorManager::DrawGraph( const char* name, Vector4 color, UtilArray values )
{
	const std::vector<float> plotValues = values.GetData<float>();
	const float MARKERS[] = { 30.0f, 60.0f, 144.0f };
	const int MARKER_COUNT = 3;
	const int sampleCount = static_cast<const int>( plotValues.size() );

	auto startPos = ImGui::GetCursorPos();

	ImPlot::PushStyleVar( ImPlotStyleVar_PlotPadding, { 0, 0 } );
	ImPlot::PushStyleVar( ImPlotStyleVar_LineWeight, 1.0f );
	ImPlot::PushStyleColor( ImPlotCol_PlotBg, { color.x, color.y, color.z, color.w } );
	ImPlot::PushStyleColor( ImPlotCol_Line, { color.x, color.y, color.z, color.w * 4.0f } );
	ImPlot::PushStyleColor( ImPlotCol_InlayText, { color.x, color.y, color.z, color.w * 4.0f } );

	if ( ImPlot::BeginPlot(
	         name, { -1, 128 }, ImPlotFlags_NoInputs | ImPlotFlags_NoMenus | ImPlotFlags_NoTitle | ImPlotFlags_NoMouseText ) )
	{
		ImPlot::SetupAxis( ImAxis_X1, 0, ImPlotAxisFlags_NoDecorations );
		ImPlot::SetupAxis( ImAxis_Y1, 0, ImPlotAxisFlags_NoDecorations );
		ImPlot::SetupAxisLimits( ImAxis_Y1, 0.0, MARKERS[MARKER_COUNT - 1] + 20.0f, ImPlotCond_Always );
		ImPlot::SetupAxisLimits( ImAxis_X1, 0.0, sampleCount, ImPlotCond_Always );

		ImPlot::PlotInfLines<float>( "##reference", MARKERS, MARKER_COUNT, ImPlotInfLinesFlags_Horizontal );

		for ( auto& marker : MARKERS )
		{
			std::string str = std::to_string( ( int )marker ) + "fps";
			float x = sampleCount - 40.0f;
			float y = marker - 15.0f;
			ImPlot::PlotText( str.c_str(), x, y );
		}

		ImPlot::PushStyleVar( ImPlotStyleVar_LineWeight, 2.0f );
		ImPlot::PlotLine<float>( name, plotValues.data(), sampleCount );
		ImPlot::PopStyleVar();
		ImPlot::EndPlot();
	}

	ImPlot::PopStyleColor( 3 );
	ImPlot::PopStyleVar( 2 );
}
