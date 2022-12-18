#pragma once
#include <baseentity.h>
#include <defs.h>
#include <edict.h>
#include <globalvars.h>
#include <imgui.h>
#include <rendermanager.h>
#include <spdlog/spdlog.h>
#include <sstream>
#include <vulkan/vulkan.h>

//@InteropGen generate class
namespace Editor
{
	inline void End()
	{
		ImGui::End();
	};

	inline void Separator()
	{
		ImGui::Dummy( ImVec2( 0, 4 ) );
		ImGui::PushStyleColor( ImGuiCol_Separator, ImVec4( 0.28f, 0.28f, 0.28f, 0.29f ) );
		ImGui::Separator();
		ImGui::PopStyleColor();
		ImGui::Dummy( ImVec2( 0, 4 ) );
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
		return ImGui::Begin( name );
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

	inline bool BeginChild( const char* name )
	{
		return ImGui::BeginChild( name, { -1, -24 } );
	}

	inline void EndChild()
	{
		ImGui::EndChild();
	}

	inline bool BeginTable( const char* name, int columnCount, int flags )
	{
		return ImGui::BeginTable(
		    name, columnCount, ImGuiTableFlags_Borders | ImGuiTableFlags_RowBg | ImGuiTableFlags_PadOuterX );
	}

	inline void EndTable()
	{
		ImGui::EndTable();
	}

	inline void TableSetupColumn( const char* name, int flags, float width )
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

	inline void SetNextItemWidth( float width )
	{
		ImGui::SetNextItemWidth( width );
	}

	inline char* InputText( const char* name, char* inputBuf, int inputLength )
	{
		ImGui::InputText( name, inputBuf, inputLength );

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
		return VERSION_NAME;
	}

	//@InteropGen ignore
	inline void Draw() {} // Do nothing - we're re-writing this in C#

	//@InteropGen ignore
	// inline void Draw()
	//{
	//	if ( ImGui::Begin( "Entities" ) )
	//	{
	//		// List all entities
	//		g_entityDictionary->ForEach( []( std::shared_ptr<BaseEntity> entity ) {
	//			std::vector<std::string> flags = {};

	//			// Get flag string based on entity flags
	//			if ( entity->HasFlag( ENTITY_MANAGED ) )
	//				flags.push_back( "ENTITY_MANAGED" );
	//			if ( entity->HasFlag( ENTITY_RENDERABLE ) )
	//				flags.push_back( "ENTITY_RENDERABLE" );

	//			// Join flags together as a comma-separated string

	//			std::string flagString = "";
	//			for ( int i = 0; i < flags.size(); i++ )
	//			{
	//				flagString += flags[i];
	//				if ( i != flags.size() - 1 )
	//					flagString += ", ";
	//			}

	//			// Display name
	//			if ( ImGui::CollapsingHeader( entity->GetName() ) )
	//			{
	//				// Basic info
	//				ImGui::Text( "\t%s", flagString.c_str() );
	//				ImGui::Text( "\t%s", entity->GetType() );
	//				ImGui::Text( "" );

	//				// Display transform
	//				auto transform = entity->GetTransform();
	//				ImGui::Text( "\tPosition: %f, %f, %f", transform.position.x, transform.position.y, transform.position.z );
	//				ImGui::Text( "\tRotation: %f, %f, %f, %f", transform.rotation.x, transform.rotation.y, transform.rotation.z,
	//				    transform.rotation.w );
	//				ImGui::Text( "\tScale: %f, %f, %f", transform.scale.x, transform.scale.y, transform.scale.z );

	//				ImGui::Text( "" );
	//			}
	//		} );
	//	}

	//	ImGui::End();

	//	if ( ImGui::Begin(
	//	         "Time", nullptr, ImGuiWindowFlags_NoDecoration | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoInputs ) )
	//	{
	//		ImGui::SetWindowPos( { 16, 16 } );
	//		ImGui::SetWindowSize( { -1, -1 } );

	//		ImGui::Text( "GPU: %s", g_renderManager->m_deviceName.c_str() );

	//		ImGui::Text( "Current time: %f", g_curTime );
	//		ImGui::Text( "Delta time: %f", g_frameTime );

	//		const float fps = 1.0f / g_frameTime;
	//		ImGui::Text( "FPS: %f", fps );
	//	}

	//	ImGui::End();

	//	if ( ImGui::Begin( "Console" ) )
	//	{
	//		if ( ImGui::BeginChild( "##console_output", { -1, -24 } ) )
	//		{
	//			ImGui::BeginTable(
	//			    "##console_output_table", 3, ImGuiTableFlags_Borders | ImGuiTableFlags_RowBg | ImGuiTableFlags_PadOuterX );

	//			ImGui::TableSetupColumn( "Time", ImGuiTableColumnFlags_WidthFixed, 64.0f );
	//			ImGui::TableSetupColumn( "Logger", ImGuiTableColumnFlags_WidthFixed, 64.0f );
	//			// ImGui::TableSetupColumn( "Level", ImGuiTableColumnFlags_WidthFixed, 64.0f );
	//			ImGui::TableSetupColumn( "Text", ImGuiTableColumnFlags_WidthStretch, 1.0f );

	//			for ( auto& item : g_logManager->m_logHistory )
	//			{
	//				ImGui::TableNextRow();
	//				ImGui::TableNextColumn();
	//				ImGui::PushStyleColor( ImGuiCol_Text, { 1.0f, 1.0f, 1.0f, 0.5f } );
	//				ImGui::Text( "%s", item.time.c_str() );
	//				ImGui::PopStyleColor();

	//				ImGui::TableNextColumn();
	//				ImGui::Text( "%s", item.logger.c_str() );

	//				// ImGui::TableNextColumn();
	//				// ImGui::Text( "%s", item.level.c_str() );

	//				ImGui::TableNextColumn();
	//				ImGui::TextWrapped( item.message.c_str() );
	//			}

	//			ImGui::EndTable();

	//			ImGui::SetScrollHereY( 1.0f );
	//			ImGui::EndChild();
	//		}

	//		bool shouldSubmit = false;

	//		const int MAX_INPUT_LENGTH = 512;
	//		static char inputBuf[MAX_INPUT_LENGTH];
	//		ImGui::SetNextItemWidth( -60 );

	//		if ( ImGui::IsWindowFocused() && !ImGui::IsAnyItemActive() && !ImGui::IsMouseClicked( 0 ) )
	//			ImGui::SetKeyboardFocusHere( 0 );

	//		ImVec2 inputPos = ImGui::GetCursorPos();
	//		inputPos.x += ImGui::GetWindowPos().x;
	//		inputPos.x += ImGui::GetWindowPos().y;

	//		if ( ImGui::InputText( "##console_input", inputBuf, MAX_INPUT_LENGTH, ImGuiInputTextFlags_EnterReturnsTrue ) )
	//			shouldSubmit = true;
	//
	//		ImGui::SameLine();

	//		if ( ImGui::Button( "Submit" ) )
	//			shouldSubmit = true;
	//
	//		if ( shouldSubmit )
	//		{
	//			spdlog::info( "> {}", inputBuf );

	//			std::string inputString = std::string( inputBuf );

	//			// Clear inputBuf
	//			memset( inputBuf, 0, sizeof( inputBuf ) );

	//			std::stringstream ss( inputString );

	//			std::string cvarName, cvarValue;
	//			ss >> cvarName >> cvarValue;

	//			std::stringstream valueStream( cvarValue );

	//			if ( !CVarManager::Instance().Exists( cvarName ) )
	//			{
	//				if ( cvarName == "list" )
	//				{
	//					CVarManager::Instance().ForEach( cvarValue, []( CVarEntry& cvar ) {
	//						std::string valueStr = CVarManager::Instance().ToString( cvar.m_name );
	//						std::string typeStr = cvar.m_value.type().name();
	//						spdlog::info( "{} - {}: {} ({})", cvar.m_name, cvar.m_description, valueStr, typeStr );
	//					} );
	//				}
	//				else
	//				{
	//					spdlog::info( "{} is not a valid command or variable", cvarName );
	//				}
	//			}
	//			else
	//			{
	//				if ( valueStream.str().size() > 0 )
	//				{
	//					CVarManager::Instance().FromString( cvarName, cvarValue );
	//				}
	//				else
	//				{
	//					cvarValue = CVarManager::Instance().ToString( cvarName );
	//					spdlog::info( "{} is '{}'", cvarName, cvarValue );
	//				}
	//			}
	//		}
	//	}

	//	ImGui::End();
	//}
} // namespace Editor
