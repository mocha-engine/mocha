#pragma once
#ifdef _IMGUI
#include <baseentity.h>
#include <edict.h>
#include <globalvars.h>
#include <spdlog/spdlog.h>
#include <thirdparty/imgui/imgui.h>

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
		if ( ImGui::Begin( "Native" ) )
		{
			ImGui::Text( "Entities:" );

			// List all entities
			g_entityDictionary->ForEach( []( std::shared_ptr<BaseEntity> entity ) {
				std::string flagStr = "";

				// Get flag string based on entity flags
				if ( entity->HasFlag( ENTITY_MANAGED ) )
					flagStr += "M";
				if ( entity->HasFlag( ENTITY_RENDERABLE ) )
					flagStr += "R";
				
				ImGui::Text( "[%s] %s", flagStr.c_str(), entity->GetName() );
			} );

			ImGui::End();
		}
	}
} // namespace Editor

#endif