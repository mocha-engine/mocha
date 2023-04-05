#pragma once

#include <Managed/managedcallbackdispatchinfo.h>
#include <Misc/subsystem.h>
#include <Windows.h>
#include <assert.h>
#include <atomic>
#include <coreclr_delegates.h>
#include <generated/UnmanagedArgs.generated.h>
#include <hostfxr.h>
#include <iostream>
#include <nethost.h>
#include <spdlog/spdlog.h>
#include <string>
#include <tuple>

using string_t = std::basic_string<char_t>;

typedef int( CORECLR_DELEGATE_CALLTYPE* RunFn )( UnmanagedArgs* args );

struct CVarManagedCmdDispatchInfo;

template <typename T>
struct CVarManagedVarDispatchInfo;

namespace HostGlobals
{
	// Globals to hold hostfxr exports
	inline hostfxr_initialize_for_runtime_config_fn init;
	inline hostfxr_get_runtime_delegate_fn getDelegate;
	inline hostfxr_set_runtime_property_value_fn setProperty;
	inline hostfxr_close_fn close;

	void* LoadHostLibrary( const char_t* path );
	void* GetExport( void* h, const char* name );
	bool LoadHostFxr();
	load_assembly_and_get_function_pointer_fn GetDotnetLoadAssembly( const char_t* configPath );

}; // namespace HostGlobals

inline static std::atomic<bool> IsAssemblyLoaded = false;
inline static std::atomic<load_assembly_and_get_function_pointer_fn> LoadFnPtr;

class HostManager : ISubSystem
{
private:
	load_assembly_and_get_function_pointer_fn m_loadAssemblyFunction;

	std::wstring m_dllPath;
	std::wstring m_configPath;
	std::wstring m_signature;

	void Invoke( std::string _method, void* params = nullptr, const char_t* delegateTypeName = UNMANAGEDCALLERSONLY_METHOD ) const;
public:
	HostManager();

	void Startup();
	void Shutdown();

	void Update() const;
	void Render() const;
	void DrawEditor() const;
	
	void FireEvent( std::string eventName ) const;
	void InvokeCallback( Handle callbackHandle, int argsCount, void* args ) const;

	// TODO: Remove all below
	void DispatchCommand( CVarManagedCmdDispatchInfo info );
	void DispatchStringCVarCallback( CVarManagedVarDispatchInfo<const char*> info );
	void DispatchFloatCVarCallback( CVarManagedVarDispatchInfo<float> info );
	void DispatchBoolCVarCallback( CVarManagedVarDispatchInfo<bool> info );
	void DispatchIntCVarCallback( CVarManagedVarDispatchInfo<int> info );
};