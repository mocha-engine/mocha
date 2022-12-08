#pragma once
#ifdef _IMGUI
#include <baseentity.h>
#include <cvarmanager.h>
#include <edict.h>
#include <globalvars.h>
#include <spdlog/spdlog.h>
#include <sstream>
#include <thirdparty/imgui/imgui.h>
#include <vulkan/rendermanager.h>

//@InteropGen generate class
namespace Editor
{
	inline void* GetImGuiContext()
	{
		auto context = ( void* )ImGui::GetCurrentContext();
		spdlog::info( "Native imgui context: {}", context );

		return context;
	}

	inline void Draw()
	{
		if ( ImGui::Begin( "Entities" ) )
		{
			// List all entities
			g_entityDictionary->ForEach( []( std::shared_ptr<BaseEntity> entity ) {
				std::vector<std::string> flags = {};

				// Get flag string based on entity flags
				if ( entity->HasFlag( ENTITY_MANAGED ) )
					flags.push_back( "ENTITY_MANAGED" );
				if ( entity->HasFlag( ENTITY_RENDERABLE ) )
					flags.push_back( "ENTITY_RENDERABLE" );

				// Join flags together as a comma-separated string

				std::string flagString = "";
				for ( int i = 0; i < flags.size(); i++ )
				{
					flagString += flags[i];
					if ( i != flags.size() - 1 )
						flagString += ", ";
				}

				// Display name
				if ( ImGui::CollapsingHeader( entity->GetName() ) )
				{
					// Basic info
					ImGui::Text( "\t%s", flagString.c_str() );
					ImGui::Text( "\t%s", entity->GetType() );
					ImGui::Text( "" );

					// Display transform
					auto transform = entity->GetTransform();
					ImGui::Text( "\tPosition: %f, %f, %f", transform.position.x, transform.position.y, transform.position.z );
					ImGui::Text( "\tRotation: %f, %f, %f, %f", transform.rotation.x, transform.rotation.y, transform.rotation.z,
					    transform.rotation.w );
					ImGui::Text( "\tScale: %f, %f, %f", transform.scale.x, transform.scale.y, transform.scale.z );

					ImGui::Text( "" );
				}
			} );

			ImGui::End();
		}

		if ( ImGui::Begin(
		         "Time", nullptr, ImGuiWindowFlags_NoDecoration | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoInputs ) )
		{
			ImGui::SetWindowPos( { 16, 16 } );
			ImGui::SetWindowSize( { -1, -1 } );

			ImGui::Text( "GPU: %s", g_renderManager->m_deviceName.c_str() );

			ImGui::Text( "Current time: %f", g_curTime );
			ImGui::Text( "Delta time: %f", g_frameTime );

			const float fps = 1.0f / g_frameTime;
			ImGui::Text( "FPS: %f", fps );

			ImGui::End();
		}

		if ( ImGui::Begin( "Console" ) )
		{
			if ( ImGui::BeginChild( "##console_output", { -1, -24 } ) )
			{
				ImGui::BeginTable(
				    "##console_output_table", 3, ImGuiTableFlags_Borders | ImGuiTableFlags_RowBg | ImGuiTableFlags_PadOuterX );

				ImGui::TableSetupColumn( "Time", ImGuiTableColumnFlags_WidthFixed, 64.0f );
				ImGui::TableSetupColumn( "Logger", ImGuiTableColumnFlags_WidthFixed, 64.0f );
				// ImGui::TableSetupColumn( "Level", ImGuiTableColumnFlags_WidthFixed, 64.0f );
				ImGui::TableSetupColumn( "Text", ImGuiTableColumnFlags_WidthStretch, 1.0f );

				for ( auto& item : g_logManager->m_logHistory )
				{
					ImGui::TableNextRow();
					ImGui::TableNextColumn();
					ImGui::PushStyleColor( ImGuiCol_Text, { 1.0f, 1.0f, 1.0f, 0.5f } );
					ImGui::Text( "%s", item.time.c_str() );
					ImGui::PopStyleColor();

					ImGui::TableNextColumn();
					ImGui::Text( "%s", item.logger.c_str() );

					// ImGui::TableNextColumn();
					// ImGui::Text( "%s", item.level.c_str() );

					ImGui::TableNextColumn();
					ImGui::TextWrapped( item.message.c_str() );
				}

				ImGui::EndTable();

				ImGui::SetScrollHereY( 1.0f );
				ImGui::EndChild();
			}

			bool shouldSubmit = false;

			const int MAX_INPUT_LENGTH = 512;
			static char inputBuf[MAX_INPUT_LENGTH];
			ImGui::SetNextItemWidth( -60 );

			if ( ImGui::IsWindowFocused() && !ImGui::IsAnyItemActive() && !ImGui::IsMouseClicked( 0 ) )
				ImGui::SetKeyboardFocusHere( 0 );

			if ( ImGui::InputText( "##console_input", inputBuf, MAX_INPUT_LENGTH, ImGuiInputTextFlags_EnterReturnsTrue ) )
				shouldSubmit = true;

			ImGui::SameLine();

			if ( ImGui::Button( "Submit" ) )
				shouldSubmit = true;

			if ( shouldSubmit )
			{
				spdlog::info( "> {}", inputBuf );

				std::string inputString = std::string( inputBuf );

				// Clear inputBuf
				memset( inputBuf, 0, sizeof( inputBuf ) );

				std::stringstream ss( inputString );

				std::string cvarName, cvarValue;
				ss >> cvarName >> cvarValue;

				std::stringstream valueStream( cvarValue );

				if (!CVarManager::Instance().Exists(cvarName))
				{
					spdlog::info( "{} is not a valid command or variable", cvarName );
				}
				else
				{
					if ( valueStream.str().size() > 0 )
					{
						CVarManager::Instance().FromString( cvarName, cvarValue );
					}
					else
					{
						cvarValue = CVarManager::Instance().ToString( cvarName );
						spdlog::info( "{} is '{}'", cvarName, cvarValue );
					}				
				}
			}

			ImGui::End();
		}

		if ( ImGui::Begin( "CVars" ) )
		{
			CVarManager::Instance().ForEach( []( CVarEntry& cvar ) {
				if ( ImGui::CollapsingHeader( cvar.m_name.c_str() ) )
				{
					std::string valueStr = CVarManager::Instance().ToString( cvar.m_name );

					ImGui::Text( "Name: %s", cvar.m_name.c_str() );
					ImGui::Text( "Description: %s", cvar.m_description.c_str() );
					ImGui::Text( "Value: %s", valueStr.c_str() );
				}
			} );

			ImGui::End();
		}
	}
} // namespace Editor

#endif