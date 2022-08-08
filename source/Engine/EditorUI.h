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
		ImGui::PushStyleColor( ImGuiCol_Text, ImVec4( 1, 1, 1, 0.5f ) );
		ImGui::Text( text.c_str() );
		ImGui::PopStyleColor();
	};

	inline void Title( std::string headerText, std::string lightText )
	{
		TextSubheading( headerText );
		TextLight( lightText );

		Separator();
	};

	inline bool Button( std::string text ) { return ImGui::Button( text.c_str() ); };

	inline bool Begin( std::string name ) { return ImGui::Begin( name.c_str() ); };

	inline void ShowDemoWindow() { ImGui::ShowDemoWindow(); };

	inline void BeginMainMenuBar()
	{
		ImGui::PushStyleVar( ImGuiStyleVar_WindowPadding, ImVec2( 0, 16 ) );
		ImGui::PushStyleColor( ImGuiCol_WindowBg, ImVec4( 0, 0, 0, 1 ) );
		ImGui::BeginMainMenuBar();
	};

	inline void EndMainMenuBar()
	{
		ImGui::EndMainMenuBar();
		ImGui::PopStyleColor();
		ImGui::PopStyleVar();
	};

	inline void SetCursorPosXRelative( int relPos ) { ImGui::SetCursorPosX( ImGui::GetCursorPosX() + relPos ); };

	inline void SetCursorPosYRelative( int relPos ) { ImGui::SetCursorPosY( ImGui::GetCursorPosY() + relPos ); };

	inline bool BeginMenu( std::string name )
	{
		SetCursorPosXRelative( 4 );
		ImGui::SetNextWindowSize( ImVec2( 250, -1 ) );

		return ImGui::BeginMenu( name.c_str() );
	};

	inline void EndMenu() { ImGui::EndMenu(); };

	inline bool MenuItem( std::string icon, std::string name )
	{
		SetCursorPosYRelative( -4 );
		auto drawList = ImGui::GetForegroundDrawList();
		auto windowPos = ImGui::GetWindowPos();
		auto windowSize = ImGui::GetWindowSize();

		auto padding = ImVec2( 8, 8 );

		auto size = ImVec2( windowSize.x - ( padding.x + 16 ), ImGui::CalcTextSize( name.c_str() ).y );
		size.x += padding.x;
		size.y += padding.y;

		bool result = ImGui::InvisibleButton( name.c_str(), size );
		SetCursorPosYRelative( -size.y );

		auto p0 = ImGui::GetCursorPos();
		p0.x += windowPos.x;
		p0.y += windowPos.y - 2;

		auto p1 = p0;
		p1.x += padding.x + size.x;
		p1.y += padding.x + size.y + 4;

		auto col = ImGui::GetColorU32( ImVec4( 0, 0, 0, 0.1f ) );

		if ( ImGui::IsItemHovered() )
			drawList->AddRectFilled( p0, p1, col );

		SetCursorPosXRelative( padding.x * 0.5f );

		ImGui::PushFont( g_Imgui->mSubheadingFont );
		ImGui::Text( icon.c_str() );
		ImGui::SameLine();
		ImGui::PopFont();

		SetCursorPosYRelative( 4 );
		ImGui::Text( name.c_str() );

		if ( result )
		{
			ImGui::CloseCurrentPopup();
		}

		return result;
	};

} // namespace EditorUI