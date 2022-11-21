#pragma once
#ifdef _IMGUI
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
			ImGui::Text( "Hello from Native C++ :3" );
			ImGui::End();
		}
	}
} // namespace Editor

#endif