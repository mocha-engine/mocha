#include "hostmanager.h"

#include <Misc/cvarmanager.h>

void* HostGlobals::load_library( const char_t* path )
{
	HMODULE h = ::LoadLibraryW( path );
	assert( h != nullptr );
	return ( void* )h;
}

void* HostGlobals::get_export( void* h, const char* name )
{
	void* f = ::GetProcAddress( ( HMODULE )h, name );
	assert( f != nullptr );
	return f;
}

bool HostGlobals::LoadHostFxr()
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
	set_property_fptr = ( hostfxr_set_runtime_property_value_fn )get_export( lib, "hostfxr_set_runtime_property_value" );
	close_fptr = ( hostfxr_close_fn )get_export( lib, "hostfxr_close" );

	return ( init_fptr && get_delegate_fptr && close_fptr );
}

load_assembly_and_get_function_pointer_fn HostGlobals::GetDotnetLoadAssembly( const char_t* configPath )
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

	// Get current working directory
	char_t cwd[MAX_PATH];
	DWORD cwd_len = GetCurrentDirectoryW( MAX_PATH, cwd );
	if ( cwd_len == 0 )
	{
		spdlog::error( "Failed to get current directory" );
		close_fptr( cxt );
		return nullptr;
	}

	// Add "build" to cwd
	std::wstring buildPath = cwd;
	buildPath += L"\\build";

	// Set CoreCLR properties
	set_property_fptr( cxt, L"APP_CONTEXT_BASE_DIRECTORY", buildPath.c_str() );
	set_property_fptr( cxt, L"APP_PATHS", buildPath.c_str() );
	set_property_fptr( cxt, L"APP_NI_PATHS", buildPath.c_str() );
	set_property_fptr( cxt, L"NATIVE_DLL_SEARCH_DIRECTORIES", buildPath.c_str() );
	set_property_fptr( cxt, L"PLATFORM_RESOURCE_ROOTS", buildPath.c_str() );

	// Get the load assembly function pointer
	rc = get_delegate_fptr( cxt, hdt_load_assembly_and_get_function_pointer, &load_assembly_and_get_function_pointer );
	if ( rc != 0 || load_assembly_and_get_function_pointer == nullptr )
		spdlog::error( "Get delegate failed: 0x{:x}", rc );

	close_fptr( cxt );
	return ( load_assembly_and_get_function_pointer_fn )load_assembly_and_get_function_pointer;
}

HostManager::HostManager()
{
	// TODO: Hardcoding these might be a bad idea?
	std::wstring basePath = L".\\build\\Mocha.Hotload";
	std::wstring signature = L"Mocha.Hotload.Main, Mocha.Hotload";

	m_dllPath = basePath + L".dll";
	m_configPath = basePath + L".runtimeconfig.json";
	m_signature = signature;

	if ( !IsAssemblyLoaded.load() )
	{
		IsAssemblyLoaded.store( true );
		LoadFnPtr.store( HostGlobals::GetDotnetLoadAssembly( m_configPath.c_str() ) );
	}
}

void HostManager::Update()
{
	Invoke( "Update" );
}

void HostManager::Render()
{
	Invoke( "Render" );
}

void HostManager::Startup()
{
	Invoke( "Run", ( void* )&args );
}

void HostManager::DrawEditor()
{
	Invoke( "DrawEditor" );
}

void HostManager::Shutdown() {}

void HostManager::FireEvent( std::string eventName )
{
	Invoke( "FireEvent", ( void* )eventName.c_str() );
}

void HostManager::DispatchCommand( CVarManagedCmdDispatchInfo info )
{
	Invoke( "DispatchCommand", &info );
}

void HostManager::DispatchStringCVarCallback( CVarManagedVarDispatchInfo<const char*> info )
{
	Invoke( "DispatchStringCVarCallback", &info );
}

void HostManager::DispatchFloatCVarCallback( CVarManagedVarDispatchInfo<float> info )
{
	Invoke( "DispatchFloatCVarCallback", &info );
}

void HostManager::DispatchBoolCVarCallback( CVarManagedVarDispatchInfo<bool> info )
{
	Invoke( "DispatchBoolCVarCallback", &info );
}

void HostManager::DispatchIntCVarCallback( CVarManagedVarDispatchInfo<int> info )
{
	Invoke( "DispatchIntCVarCallback", &info );
}

inline void HostManager::Invoke( std::string _method, void* params, const char_t* delegateTypeName )
{
	// Convert to std::wstring
	std::wstring method( _method.begin(), _method.end() );

	// Function pointer to managed delegate
	void* fnPtr = nullptr;

	int rc =
	    LoadFnPtr.load()( m_dllPath.c_str(), m_signature.c_str(), method.c_str(), delegateTypeName, nullptr, ( void** )&fnPtr );

	if ( fnPtr == nullptr )
	{
		spdlog::error( "Failed to load managed method {}", _method );
	}

	// Invoke method
	( ( void ( * )( void* ) )fnPtr )( params );
}