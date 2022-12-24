#pragma once
#include <backends/imgui_impl_vulkan.h>
#include <baseentity.h>
#include <defs.h>
#include <edict.h>
#include <fontawesome.h>
#include <globalvars.h>
#include <imgui.h>
#include <rendermanager.h>
#include <spdlog/spdlog.h>
#include <sstream>
#include <texture.h>
#include <vulkan/vulkan.h>

//@InteropGen generate class
namespace Editor
{
	inline void SeparatorH()
	{
		ImGui::Dummy( ImVec2( 0, 4 ) );
		ImGui::PushStyleColor( ImGuiCol_Separator, ImVec4( 0.28f, 0.28f, 0.28f, 0.29f ) );
		ImGui::Separator();
		ImGui::PopStyleColor();
		ImGui::Dummy( ImVec2( 0, 4 ) );
	};

	inline void SeparatorV()
	{
		ImGui::Dummy( ImVec2( 8, 0 ) );
		ImGui::SameLine();
	};

	inline void Text( const char* text )
	{
		ImGui::Text( "%s", text );
	};

	inline void TextWrapped( const char* text )
	{
		ImGui::TextWrapped( "%s", text );
	};

	inline void TextBold( const char* text )
	{
		// ImGui::PushFont( g_Imgui->mBoldFont );
		Text( text );
		// ImGui::PopFont();
	};

	inline void TextSubheading( const char* text )
	{
		// ImGui::PushFont( g_Imgui->mSubheadingFont );
		Text( text );
		ImGui::Dummy( ImVec2( 0, 2 ) );
		// ImGui::PopFont();
	};

	inline void TextHeading( const char* text )
	{
		// ImGui::PushFont( g_Imgui->mHeadingFont );
		Text( text );
		ImGui::Dummy( ImVec2( 0, 2 ) );
		// ImGui::PopFont();
	};

	inline void TextMonospace( const char* text )
	{
		// ImGui::PushFont( g_Imgui->mMonospaceFont );
		Text( text );
		// ImGui::PopFont();
	};

	inline void TextLight( const char* text )
	{
		ImGui::PushStyleColor( ImGuiCol_Text, ImVec4( 1, 1, 1, 0.75f ) );
		Text( text );
		ImGui::PopStyleColor();
	}

	inline bool BeginMainMenuBar()
	{
		return ImGui::BeginMainMenuBar();
	}

	inline bool MenuItem( const char* text )
	{
		return ImGui::MenuItem( text );
	}

	inline bool BeginMenu( const char* text )
	{
		return ImGui::BeginMenu( text );
	}

	inline void EndMenu()
	{
		return ImGui::EndMenu();
	}

	inline void EndMainMenuBar()
	{
		ImGui::EndMainMenuBar();
	}

	inline bool Button( const char* text )
	{
		return ImGui::Button( text );
	};

	inline bool Begin( const char* name )
	{
		return ImGui::Begin( name, nullptr,
		    ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_AlwaysUseWindowPadding | ImGuiWindowFlags_NoScrollWithMouse |
		        ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoResize );
	};

	inline void End()
	{
		// ImGui::EndChild();
		ImGui::End();
	};

	inline void ShowDemoWindow()
	{
		ImGui::ShowDemoWindow();
	};

	inline bool CollapsingHeader( const char* name )
	{
		return ImGui::CollapsingHeader( name );
	}

	inline bool BeginOverlay( const char* name )
	{
		ImGui::SetNextWindowViewport( ImGui::GetMainViewport()->ID );

		bool b = ImGui::Begin(
		    name, nullptr, ImGuiWindowFlags_NoDecoration | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoInputs );

		auto workPos = ImGui::GetMainViewport()->WorkPos;

		ImGui::SetWindowPos( { workPos.x + 16, workPos.y + 16 } );
		ImGui::SetWindowSize( { -1, -1 } );

		return b;
	}

	inline const char* GetGPUName()
	{
		return g_renderManager->m_deviceName.c_str();
	}

	inline bool BeginChild( const char* name, int width, int height )
	{
		return ImGui::BeginChild( name, { ( float )width, ( float )height }, false, ImGuiWindowFlags_AlwaysUseWindowPadding );
	}

	inline void EndChild()
	{
		ImGui::EndChild();
	}

	inline bool BeginTable( const char* name, int columnCount, int flags )
	{
		return ImGui::BeginTable( name, columnCount,
		    ImGuiTableFlags_Borders | ImGuiTableFlags_RowBg | ImGuiTableFlags_PadOuterX | ImGuiTableFlags_Resizable );
	}

	inline void EndTable()
	{
		ImGui::EndTable();
	}

	inline void TableSetupStretchColumn( const char* name )
	{
		ImGui::TableSetupColumn( name, ImGuiTableColumnFlags_WidthStretch, 1.0f );
	}

	inline void TableSetupFixedColumn( const char* name, float width )
	{
		ImGui::TableSetupColumn( name, ImGuiTableColumnFlags_WidthFixed, width );
	}

	inline void TableNextRow()
	{
		ImGui::TableNextRow();
	}

	inline void TableNextColumn()
	{
		ImGui::TableNextColumn();
	}

	inline void TableHeaders()
	{
		ImGui::TableHeadersRow();
	}

	inline void SetNextItemWidth( float width )
	{
		ImGui::SetNextItemWidth( width );
	}

	inline char* InputText( const char* name, char* inputBuf, int inputLength )
	{
		ImGui::InputText( name, inputBuf, inputLength, ImGuiInputTextFlags_EnterReturnsTrue );

		return inputBuf;
	}

	inline void SameLine()
	{
		ImGui::SameLine();
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
		VkExtent2D extent = g_renderManager->GetWindowExtent();
		return { ( float )extent.width, ( float )extent.height };
	}

	inline const char* GetVersionName()
	{
		return GAME_VERSION;
	}

	inline Vector3 DragFloat3( const char* name, Vector3 value )
	{
		float v[3] = { value.x, value.y, value.z };
		ImGui::DragFloat3( name, v );
		return { v[0], v[1], v[2] };
	}

	inline bool Selectable( const char* name )
	{
		return ImGui::Selectable( name );
	}

	inline void Image( Texture* texture, int x, int y )
	{
		ImGui::Image( texture->GetImGuiID(), { ( float )x, ( float )y } );
	}

	inline void SetCursorPos( float x, float y )
	{
		ImGui::SetCursorPos( { x, y } );
	}

	inline float GetCursorX()
	{
		return ImGui::GetCursorPosX();
	}

	inline float GetCursorY()
	{
		return ImGui::GetCursorPosY();
	}

	inline void SetCursorX( float x )
	{
		ImGui::SetCursorPosX( x );
	}

	inline void SetCursorY( float y )
	{
		ImGui::SetCursorPosY( y );
	}

	inline void BumpCursorX( float x )
	{
		float curr = ImGui::GetCursorPosX();
		ImGui::SetCursorPosX( curr + x );
	}

	inline void BumpCursorY( float y )
	{
		float curr = ImGui::GetCursorPosY();
		ImGui::SetCursorPosY( curr + y );
	}

	inline float GetColumnWidth()
	{
		return ImGui::GetColumnWidth();
	}
} // namespace Editor
