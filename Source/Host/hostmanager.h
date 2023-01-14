#pragma once

#include <Windows.h>
#include <assert.h>
#include <coreclr_delegates.h>
#include <filesystemwatcher.h>
#include <generated/UnmanagedArgs.generated.h>
#include <hostfxr.h>
#include <iostream>
#include <nethost.h>
#include <spdlog/spdlog.h>
#include <string>
#include <subsystem.h>
#include <tuple>

using string_t = std::basic_string<char_t>;

typedef int( CORECLR_DELEGATE_CALLTYPE* run_fn )( UnmanagedArgs* args );

namespace HostGlobals
{
	// Globals to hold hostfxr exports
	inline hostfxr_initialize_for_runtime_config_fn init_fptr;
	inline hostfxr_get_runtime_delegate_fn get_delegate_fptr;
	inline hostfxr_set_runtime_property_value_fn set_property_fptr;
	inline hostfxr_close_fn close_fptr;

	void* load_library( const char_t* path );
	void* get_export( void* h, const char* name );
	bool LoadHostFxr();
	load_assembly_and_get_function_pointer_fn GetDotnetLoadAssembly( const char_t* configPath );

}; // namespace HostGlobals

class HostManager : ISubSystem
{
private:
	load_assembly_and_get_function_pointer_fn m_lagfp;

	std::wstring m_dllPath;
	std::wstring m_configPath;
	std::wstring m_signature;

	std::shared_ptr<FileSystemWatcher> m_fileSystemWatcher;

	void Invoke( std::string _method, void* params = nullptr, const char_t* delegateTypeName = UNMANAGEDCALLERSONLY_METHOD );

public:
	HostManager();

	void Startup();
	void Shutdown();

	void Update();
	void Render();
	void DrawEditor();
	void FireEvent( std::string eventName );
};
