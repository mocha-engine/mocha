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
				if ( ImGui::TreeNode( entity->GetName() ) )
				{
					// Basic info
					ImGui::Text( "\t%s", flagString.c_str() );

					// Display transform
					auto transform = entity->GetTransform();
					ImGui::Text( "\tPosition: %f, %f, %f", transform.position.x, transform.position.y, transform.position.z );

					ImGui::TreePop();
				}
			} );

			ImGui::End();
		}
	}
} // namespace Editor

#endif