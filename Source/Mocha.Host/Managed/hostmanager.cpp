#include "hostmanager.h"

#include <Misc/cvarmanager.h>

void* HostGlobals::LoadHostLibrary( const char_t* path )
{
	const HMODULE h = ::LoadLibraryW( path );
	assert( h != nullptr );
	return ( void* )h;
}

void* HostGlobals::GetExport( void* h, const char* name )
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

	const int getHostfxrPathResult = get_hostfxr_path( buffer, &buffer_size, nullptr );
	if (getHostfxrPathResult != 0)
		return false;

	// Load hostfxr and get desired exports
	void* lib = LoadHostLibrary( buffer );
	init = static_cast<hostfxr_initialize_for_runtime_config_fn>( GetExport( lib,

		"hostfxr_initialize_for_runtime_config" ) );
	getDelegate = static_cast<hostfxr_get_runtime_delegate_fn>( GetExport( lib, "hostfxr_get_runtime_delegate" ) );
	setProperty = static_cast<hostfxr_set_runtime_property_value_fn>( GetExport( lib,
		"hostfxr_set_runtime_property_value" ) );
	close = static_cast<hostfxr_close_fn>( GetExport( lib, "hostfxr_close" ) );

	return ( init && getDelegate && close );
}

load_assembly_and_get_function_pointer_fn HostGlobals::GetDotnetLoadAssembly( const char_t* configPath )
{
	LoadHostFxr();

	// Load .NET Core
	void* load_assembly_and_get_function_pointer = nullptr;
	hostfxr_handle cxt = nullptr;
	int rc = init( configPath, nullptr, &cxt );
	if (rc != 0 || cxt == nullptr)
	{
		spdlog::error( "Failed to initialize: 0x{:x}", rc );
		close( cxt );
		return nullptr;
	}

	// Get current working directory
	char_t cwd[MAX_PATH];
	const DWORD cwdLength = GetCurrentDirectoryW( MAX_PATH, cwd );
	if (cwdLength == 0)
	{
		spdlog::error( "Failed to get current directory" );
		close( cxt );
		return nullptr;
	}

	// Add "build" to cwd
	std::wstring buildPath = cwd;
	buildPath += L"\\build";

	// Set CoreCLR properties
	setProperty( cxt, L"APP_CONTEXT_BASE_DIRECTORY", buildPath.c_str() );
	setProperty( cxt, L"APP_PATHS", buildPath.c_str() );
	setProperty( cxt, L"APP_NI_PATHS", buildPath.c_str() );
	setProperty( cxt, L"NATIVE_DLL_SEARCH_DIRECTORIES", buildPath.c_str() );
	setProperty( cxt, L"PLATFORM_RESOURCE_ROOTS", buildPath.c_str() );

	// Get the load assembly function pointer
	rc = getDelegate( cxt, hdt_load_assembly_and_get_function_pointer, &load_assembly_and_get_function_pointer );
	if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
		spdlog::error( "Get delegate failed: 0x{:x}", rc );

	close( cxt );
	return static_cast<load_assembly_and_get_function_pointer_fn>( load_assembly_and_get_function_pointer );
}

HostManager::HostManager()
{
	// TODO: Hardcoding these might be a bad idea?
	std::wstring basePath = L".\\build\\Mocha.Hotload";
	std::wstring signature = L"Mocha.Hotload.Main, Mocha.Hotload";

	m_dllPath = basePath + L".dll";
	m_configPath = basePath + L".runtimeconfig.json";
	m_signature = signature;

	if (!IsAssemblyLoaded.load())
	{
		IsAssemblyLoaded.store( true );
		LoadFnPtr.store( HostGlobals::GetDotnetLoadAssembly( m_configPath.c_str() ) );
	}
}

void HostManager::Update() const
{
	Invoke( "Update" );
}

void HostManager::Render() const
{
	Invoke( "Render" );
}

void HostManager::DrawEditor() const
{
	Invoke( "DrawEditor" );
}

void HostManager::Startup()
{
	Invoke( "Run", ( void* )&args );
}

void HostManager::Shutdown() {}

void HostManager::FireEvent( std::string eventName ) const
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

void HostManager::InvokeCallback( Handle callbackHandle, int argsCount, void* args ) const
{
	ManagedCallbackDispatchInfo dispatchInfo{};
	dispatchInfo.args = args;
	dispatchInfo.argsSize = argsCount;
	dispatchInfo.handle = callbackHandle;

	Invoke( "InvokeCallback", &dispatchInfo );
}

inline void HostManager::Invoke( std::string _method, void* params, const char_t* delegateTypeName ) const
{
	// Convert to std::wstring
	const std::wstring method( _method.begin(), _method.end() );

	// Function pointer to managed delegate
	void* fnPtr = nullptr;

	int rc =
		LoadFnPtr.load()( m_dllPath.c_str(), m_signature.c_str(), method.c_str(), delegateTypeName, nullptr, ( void** )&fnPtr );

	if (fnPtr == nullptr)
	{
		spdlog::error( "Failed to load managed method {}", _method );
	}

	// Invoke method
	static_cast<void ( *)( void* )>( fnPtr )( params );
}
