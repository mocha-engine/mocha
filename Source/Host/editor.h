#pragma once
#include <baseentity.h>
#include <defer.h>
#include <defs.h>
#include <entitymanager.h>
#include <fontawesome.h>
#include <globalvars.h>
#include <imgui.h>
#include <imgui_internal.h>
#include <implot.h>
#include <rendermanager.h>
#include <spdlog/spdlog.h>
#include <sstream>
#include <texture.h>

namespace Editor
{
	/// <summary>
	/// Get the current pointer to an ImGUI context.
	/// This is used in order to effectively "link" managed ImGUI
	/// to our native ImGUI instance.
	/// </summary>
	/// <returns></returns>
	GENERATE_BINDINGS inline void* GetContextPointer()
	{
		auto ctx = ImGui::GetCurrentContext();
		return ( void* )ctx;
	};

	GENERATE_BINDINGS inline void TextBold( const char* text )
	{
		ImGui::PushFont( g_renderContext->m_boldFont );
		ImGui::Text( "%s", text );
		ImGui::PopFont();
	};

	GENERATE_BINDINGS inline void TextSubheading( const char* text )
	{
		ImGui::PushFont( g_renderContext->m_subheadingFont );
		ImGui::Text( "%s", text );
		ImGui::Dummy( ImVec2( 0, 2 ) );
		ImGui::PopFont();
	};

	GENERATE_BINDINGS inline void TextHeading( const char* text )
	{
		ImGui::PushFont( g_renderContext->m_headingFont );
		ImGui::Text( "%s", text );
		ImGui::Dummy( ImVec2( 0, 2 ) );
		ImGui::PopFont();
	};

	GENERATE_BINDINGS inline void TextMonospace( const char* text )
	{
		ImGui::PushFont( g_renderContext->m_monospaceFont );
		ImGui::Text( "%s", text );
		ImGui::PopFont();
	};

	GENERATE_BINDINGS inline void TextLight( const char* text )
	{
		ImGui::PushStyleColor( ImGuiCol_Text, ImVec4( 1, 1, 1, 0.75f ) );
		ImGui::Text( "%s", text );
		ImGui::PopStyleColor();
	}

	GENERATE_BINDINGS inline const char* GetGPUName()
	{
		return g_renderManager->GetGPUName();
	}

	GENERATE_BINDINGS inline char* InputText( const char* name, char* inputBuf, int inputLength )
	{
		ImGui::InputText( name, inputBuf, inputLength, ImGuiInputTextFlags_EnterReturnsTrue );

		return inputBuf;
	}

	GENERATE_BINDINGS inline void RenderViewDropdown()
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

	GENERATE_BINDINGS inline Vector2 GetWindowSize()
	{
		Size2D size;
		g_renderContext->GetWindowSize( &size );
		return { ( float )size.x, ( float )size.y };
	}

	GENERATE_BINDINGS inline Vector2 GetRenderSize()
	{
		Size2D size;
		g_renderContext->GetRenderSize( &size );
		return { ( float )size.x, ( float )size.y };
	}

	GENERATE_BINDINGS inline const char* GetVersionName()
	{
		return GAME_VERSION;
	}

	GENERATE_BINDINGS inline void Image( Texture* texture, uint32_t textureWidth, uint32_t textureHeight, int x, int y )
	{
		void* imguiTextureID;
		g_renderContext->GetImGuiTextureID( &texture->m_image, &imguiTextureID );

		// Calculate new UVs based on reported textureWidth, textureHeight vs texture->m_size
		// This is done because the C++ side isn't aware of any padding applied in order to get
		// the image to become POT
		float u = ( float )textureWidth / ( float )texture->m_size.x;
		float v = ( float )textureWidth / ( float )texture->m_size.y;

		ImGui::Image( imguiTextureID, { ( float )x, ( float )y }, { 0, 0 }, { u, v } );
	}

	GENERATE_BINDINGS inline bool BeginMainStatusBar()
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

	GENERATE_BINDINGS inline void DrawGraph( const char* name, Vector4 color, UtilArray values )
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
