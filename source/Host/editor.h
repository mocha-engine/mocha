#pragma once
#include <backends/imgui_impl_vulkan.h>
#include <baseentity.h>
#include <defs.h>
#include <edict.h>
#include <fontawesome.h>
#include <globalvars.h>
#include <imgui.h>
#include <imgui_internal.h>
#include <implot.h>
#include <rendermanager.h>
#include <spdlog/spdlog.h>
#include <sstream>
#include <texture.h>
#include <volk.h>

//@InteropGen generate class
namespace Editor
{
	inline void* GetContextPointer()
	{
		auto ctx = ImGui::GetCurrentContext();
		return ( void* )ctx;
	};

	inline void TextBold( const char* text )
	{
		// ImGui::PushFont( g_Imgui->mBoldFont );
		ImGui::Text( "%s", text );
		// ImGui::PopFont();
	};

	inline void TextSubheading( const char* text )
	{
		// ImGui::PushFont( g_Imgui->mSubheadingFont );
		ImGui::Text( "%s", text );
		ImGui::Dummy( ImVec2( 0, 2 ) );
		// ImGui::PopFont();
	};

	inline void TextHeading( const char* text )
	{
		// ImGui::PushFont( g_Imgui->mHeadingFont );
		ImGui::Text( "%s", text );
		ImGui::Dummy( ImVec2( 0, 2 ) );
		// ImGui::PopFont();
	};

	inline void TextMonospace( const char* text )
	{
		// TODO
		
		// ImGui::PushFont( g_renderManager->m_monospaceFont );
		ImGui::Text( "%s", text );
		// ImGui::PopFont();
	};

	inline void TextLight( const char* text )
	{
		ImGui::PushStyleColor( ImGuiCol_Text, ImVec4( 1, 1, 1, 0.75f ) );
		ImGui::Text( "%s", text );
		ImGui::PopStyleColor();
	}

	inline const char* GetGPUName()
	{
		return "TODO";
		// return g_renderManager->m_deviceName.c_str();
	}

	inline char* InputText( const char* name, char* inputBuf, int inputLength )
	{
		ImGui::InputText( name, inputBuf, inputLength, ImGuiInputTextFlags_EnterReturnsTrue );

		return inputBuf;
	}

	inline void RenderViewDropdown()
	{
		if ( ImGui::BeginMenu( "Debug View" ) )
		{
			if ( ImGui::MenuItem( "None" ) )
				g_debugView = RenderDebugViews::NONE;

			if ( ImGui::MenuItem( "Diffuse" ) )
				g_debugView = RenderDebugViews::DIFFUSE;

			if ( ImGui::MenuItem( "Normal" ) )
				g_debugView = RenderDebugViews::NORMAL;

			if ( ImGui::MenuItem( "Ambient Occlusion" ) )
				g_debugView = RenderDebugViews::AMBIENTOCCLUSION;

			if ( ImGui::MenuItem( "Metalness" ) )
				g_debugView = RenderDebugViews::METALNESS;

			if ( ImGui::MenuItem( "Roughness" ) )
				g_debugView = RenderDebugViews::ROUGHNESS;

			if ( ImGui::MenuItem( "Other" ) )
				g_debugView = RenderDebugViews::OTHER;

			ImGui::EndMenu();
		}
	}

	inline Vector2 GetWindowSize()
	{
		return { 1280, 720 };
		// VkExtent2D extent = g_renderManager->GetWindowExtent();
		// return { ( float )extent.width, ( float )extent.height };
	}

	inline const char* GetVersionName()
	{
		return GAME_VERSION;
	}

	inline void Image( Texture* texture, int x, int y )
	{
		// ImGui::Image( texture->GetImGuiID(), { ( float )x, ( float )y } );
	}

	inline bool BeginMainStatusBar()
	{
		ImGuiViewportP* viewport = ( ImGuiViewportP* )( void* )ImGui::GetMainViewport();
		ImGuiWindowFlags window_flags =
		    ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_MenuBar;
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

	inline void DrawGraph( const char* name, Vector4 color, UtilArray values )
	{
		const std::vector<float> plotValues = values.GetData<float>();
		const float MARKERS[] = { 30.0f, 60.0f, 144.0f };
		const int MARKER_COUNT = 3;
		const int sampleCount = plotValues.size();

		auto startPos = ImGui::GetCursorPos();

		ImPlot::PushStyleVar( ImPlotStyleVar_PlotPadding, { 0, 0 } );
		ImPlot::PushStyleVar( ImPlotStyleVar_LineWeight, 1.0f );
		ImPlot::PushStyleColor( ImPlotCol_PlotBg, { color.x, color.y, color.z, color.w } );
		ImPlot::PushStyleColor( ImPlotCol_Line, { color.x, color.y, color.z, color.w * 4.0f } );
		ImPlot::PushStyleColor( ImPlotCol_InlayText, { color.x, color.y, color.z, color.w * 4.0f } );

		if ( ImPlot::BeginPlot( name, { -1, 128 },
		         ImPlotFlags_NoInputs | ImPlotFlags_NoMenus | ImPlotFlags_NoTitle | ImPlotFlags_NoMouseText ) )
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
} // namespace Editor
