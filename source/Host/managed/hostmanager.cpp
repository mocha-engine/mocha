#include "hostmanager.h"

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

	// Get the load assembly function pointer
	rc = get_delegate_fptr( cxt, hdt_load_assembly_and_get_function_pointer, &load_assembly_and_get_function_pointer );
	if ( rc != 0 || load_assembly_and_get_function_pointer == nullptr )
		spdlog::error( "Get delegate failed: 0x{:x}", rc );

	close_fptr( cxt );
	return ( load_assembly_and_get_function_pointer_fn )load_assembly_and_get_function_pointer;
}

HostManager::HostManager( std::wstring basePath, std::wstring signature )
{
	m_dllPath = basePath + L".dll";
	m_configPath = basePath + L".runtimeconfig.json";
	m_signature = signature;

	m_lagfp = HostGlobals::GetDotnetLoadAssembly( m_configPath.c_str() );
}

void HostManager::Render()
{
	Invoke( "Render" );
}

void HostManager::StartUp()
{
	Invoke( "Run", ( void* )&args );
}

void HostManager::ShutDown() {}

void HostManager::FireEvent( std::string eventName )
{
	Invoke( "FireEvent", ( void* )eventName.c_str() );
}

inline void HostManager::Invoke( std::string _method, void* params, const char_t* delegateTypeName )
{
	// Convert to std::wstring
	std::wstring method( _method.begin(), _method.end() );

	// Function pointer to managed delegate
	void* fnPtr = nullptr;

	int rc = m_lagfp( m_dllPath.c_str(), m_signature.c_str(), method.c_str(), delegateTypeName, nullptr, ( void** )&fnPtr );

	if ( fnPtr == nullptr )
	{
		spdlog::error( "Failed to load managed method {}", _method );
	}

	// Invoke method
	( ( void ( * )( void* ) )fnPtr )( params );
}