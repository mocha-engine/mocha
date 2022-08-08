#pragma once
#include "CImgui.h"
#include "Globals.h"

#include <imgui.h>
#include <string>

//@InteropGen generate class
namespace EditorUI
{
	inline void End() { ImGui::End(); };

	inline void Separator()
	{
		ImGui::Dummy( ImVec2( 0, 4 ) );
		ImGui::PushStyleColor( ImGuiCol_Separator, ImVec4( 0.28f, 0.28f, 0.28f, 0.29f ) );
		ImGui::Separator();
		ImGui::PopStyleColor();
		ImGui::Dummy( ImVec2( 0, 4 ) );
	};

	inline void Text( std::string text ) { ImGui::Text( text.c_str() ); };

	inline void TextBold( std::string text )
	{
		ImGui::PushFont( g_Imgui->mBoldFont );
		ImGui::Text( text.c_str() );
		ImGui::PopFont();
	};

	inline void TextSubheading( std::string text )
	{
		ImGui::PushFont( g_Imgui->mSubheadingFont );
		ImGui::Text( text.c_str() );
		ImGui::Dummy( ImVec2( 0, 2 ) );
		ImGui::PopFont();
	};

	inline void TextHeading( std::string text )
	{
		ImGui::PushFont( g_Imgui->mHeadingFont );
		ImGui::Text( text.c_str() );
		ImGui::Dummy( ImVec2( 0, 2 ) );
		ImGui::PopFont();
	};

	inline void TextMonospace( std::string text )
	{
		ImGui::PushFont( g_Imgui->mMonospaceFont );
		ImGui::Text( text.c_str() );
		ImGui::PopFont();
	};

	inline void TextLight( std::string text )
	{
		ImGui::PushStyleColor( ImGuiCol_Text, ImVec4( 1, 1, 1, 0.75f ) );
		ImGui::Text( text.c_str() );
		ImGui::PopStyleColor();
	}

	inline bool Button( std::string text ) { return ImGui::Button( text.c_str() ); };

	inline bool Begin( std::string name ) { return ImGui::Begin( name.c_str() ); };

	inline void ShowDemoWindow() { ImGui::ShowDemoWindow(); };

} // namespace EditorUI