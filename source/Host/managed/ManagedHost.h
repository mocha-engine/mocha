#pragma once

#include "../generated/UnmanagedArgs.generated.h"

#include <Windows.h>
#include <assert.h>
#include <coreclr_delegates.h>
#include <hostfxr.h>
#include <iostream>
#include <nethost.h>
#include <spdlog/spdlog.h>
#include <string>
#include <tuple>

using string_t = std::basic_string<char_t>;

typedef int( CORECLR_DELEGATE_CALLTYPE* run_fn )( UnmanagedArgs* args );

namespace HostGlobals
{
	// Globals to hold hostfxr exports
	inline hostfxr_initialize_for_runtime_config_fn init_fptr;
	inline hostfxr_get_runtime_delegate_fn get_delegate_fptr;
	inline hostfxr_close_fn close_fptr;

	inline void* load_library( const char_t* path )
	{
		HMODULE h = ::LoadLibraryW( path );
		assert( h != nullptr );
		return ( void* )h;
	}
	inline void* get_export( void* h, const char* name )
	{
		void* f = ::GetProcAddress( ( HMODULE )h, name );
		assert( f != nullptr );
		return f;
	}

	inline bool LoadHostFxr()
	{
		// Pre-allocate a large buffer for the path to hostfxr
		char_t buffer[MAX_PATH];
		size_t buffer_size = sizeof( buffer ) / sizeof( char_t );
		int rc = get_hostfxr_path( buffer, &buffer_size, nullptr );
		if ( rc != 0 )
			return false;

		// Load hostfxr and get desired exports
		void* lib = load_library( buffer );
		init_fptr = ( hostfxr_initialize_for_runtime_config_fn )get_export( lib, "hostfxr_initialize_for_runtime_config" );
		get_delegate_fptr = ( hostfxr_get_runtime_delegate_fn )get_export( lib, "hostfxr_get_runtime_delegate" );
		close_fptr = ( hostfxr_close_fn )get_export( lib, "hostfxr_close" );

		return ( init_fptr && get_delegate_fptr && close_fptr );
	}

	inline load_assembly_and_get_function_pointer_fn GetDotnetLoadAssembly( const char_t* configPath )
	{
		LoadHostFxr();

		// Load .NET Core
		void* load_assembly_and_get_function_pointer = nullptr;
		hostfxr_handle cxt = nullptr;
		int rc = init_fptr( configPath, nullptr, &cxt );
		if ( rc != 0 || cxt == nullptr )
		{
			spdlog::error( "Failed to initialize: 0x{:x}", rc );
			close_fptr( cxt );
			return nullptr;
		}

		// Get the load assembly function pointer
		rc = get_delegate_fptr( cxt, hdt_load_assembly_and_get_function_pointer, &load_assembly_and_get_function_pointer );
		if ( rc != 0 || load_assembly_and_get_function_pointer == nullptr )
			spdlog::error( "Get delegate failed: 0x{:x}", rc );

		close_fptr( cxt );
		return ( load_assembly_and_get_function_pointer_fn )load_assembly_and_get_function_pointer;
	}

}; // namespace HostGlobals

class ManagedHost
{
private:
	load_assembly_and_get_function_pointer_fn m_lagfp;

	std::wstring m_dllPath;
	std::wstring m_configPath;
	std::wstring m_signature;

public:
	inline ManagedHost( std::wstring basePath, std::wstring signature )
	{
		m_dllPath = basePath + L".dll";
		m_configPath = basePath + L".runtimeconfig.json";
		m_signature = signature;

		m_lagfp = HostGlobals::GetDotnetLoadAssembly( m_configPath.c_str() );
	}

	inline void Invoke( std::string _method )
	{
		// Convert to std::wstring
		std::wstring method( _method.begin(), _method.end() );

		// Function pointer to managed delegate
		run_fn fnPtr = nullptr;
		int rc = m_lagfp(
		    m_dllPath.c_str(), m_signature.c_str(), method.c_str(), UNMANAGEDCALLERSONLY_METHOD, nullptr, ( void** )&fnPtr );

		if ( fnPtr == nullptr )
		{
			spdlog::error( "Failed to load managed method {}", _method );
		}

		// Invoke method
		fnPtr( &args );
	}
};
