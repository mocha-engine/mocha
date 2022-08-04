#include "CNetCoreHost.h"
#include "CLogger.h"
#include <functional>

// TODO(AG): InteropGen this
#include "../Mocha.NeoRenderer/generated/CNativeWindow.generated.h"
#include "generated/CLogger.generated.h"

namespace
{
	hostfxr_initialize_for_runtime_config_fn init_fptr;
	hostfxr_get_runtime_delegate_fn get_delegate_fptr;
	hostfxr_close_fn close_fptr;

	bool load_hostfxr();
	load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const char_t* assembly);
}

//
// .NET loading functions
//
namespace
{
	void* load_library(const char_t* path)
	{
		HMODULE h = ::LoadLibraryW(path);
		assert(h != nullptr);
		return (void*)h;
	}
	void* get_export(void* h, const char* name)
	{
		void* f = ::GetProcAddress((HMODULE)h, name);
		assert(f != nullptr);
		return f;
	}

	bool load_hostfxr()
	{
		// Pre-allocate a large buffer for the path to hostfxr
		char_t buffer[MAX_PATH];
		size_t buffer_size = sizeof(buffer) / sizeof(char_t);
		int rc = get_hostfxr_path(buffer, &buffer_size, nullptr);
		if (rc != 0)
			return false;

		// Load hostfxr and get desired exports
		void* lib = load_library(buffer);
		init_fptr = (hostfxr_initialize_for_runtime_config_fn)get_export(lib, "hostfxr_initialize_for_runtime_config");
		get_delegate_fptr = (hostfxr_get_runtime_delegate_fn)get_export(lib, "hostfxr_get_runtime_delegate");
		close_fptr = (hostfxr_close_fn)get_export(lib, "hostfxr_close");

		return (init_fptr && get_delegate_fptr && close_fptr);
	}

	load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const char_t* config_path)
	{
		// Load .NET Core
		void* load_assembly_and_get_function_pointer = nullptr;
		hostfxr_handle cxt = nullptr;
		int rc = init_fptr(config_path, nullptr, &cxt);
		if (rc != 0 || cxt == nullptr)
		{
			std::cerr << "Init failed: " << std::hex << std::showbase << rc << std::endl;
			close_fptr(cxt);
			return nullptr;
		}

		// Get the load assembly function pointer
		rc = get_delegate_fptr(
			cxt,
			hdt_load_assembly_and_get_function_pointer,
			&load_assembly_and_get_function_pointer);
		if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
			std::cerr << "Get delegate failed: " << std::hex << std::showbase << rc << std::endl;

		close_fptr(cxt);
		return (load_assembly_and_get_function_pointer_fn)load_assembly_and_get_function_pointer;
	}
}

// TODO(AG): InteropGen this
struct UnmanagedArgs {
	CNativeWindow* CNativeWindowPtr;
	CLogger* CLoggerPtr;

	void* CreateMethodPtr;
	void* RunMethodPtr;
	void* GetWindowPointerMethodPtr;

	void* InfoMethodPtr;
	void* WarningMethodPtr;
	void* ErrorMethodPtr;
	void* TraceMethodPtr;
};

void CNetCoreHost::CallFunction(string_t config_path, string_t dll_path, string_t dotnet_type, string_t dotnet_method)
{
	CLogger logger;
	CNativeWindow window;

	// Load HostFxr and get exported hosting functions
	if (!load_hostfxr())
	{
		assert(false && "Failed to load hostfxr");
		return;
	}

	// Initialize and start the .NET Core runtime
	load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer = nullptr;
	load_assembly_and_get_function_pointer = get_dotnet_load_assembly(config_path.c_str());
	assert(load_assembly_and_get_function_pointer != nullptr && "Failure: get_dotnet_load_assembly()");

	// Create unmanaged args
	// TODO(AG): InteropGen this
	UnmanagedArgs args
	{
		&window,
		&logger,

		(void*)__CNativeWindow_Create,
		(void*)__CNativeWindow_Run,
		(void*)__CNativeWindow_GetWindowPointer,

		(void*)__CLogger_Info,
		(void*)__CLogger_Warning,
		(void*)__CLogger_Error,
		(void*)__CLogger_Trace
	};

	// Function pointer to managed delegate with non-default signature
	typedef void (CORECLR_DELEGATE_CALLTYPE* void_fn)(UnmanagedArgs*);
	void_fn function = nullptr;
	int rc = load_assembly_and_get_function_pointer(
		dll_path.c_str(),
		dotnet_type.c_str(),
		dotnet_method.c_str(),
		UNMANAGEDCALLERSONLY_METHOD,
		nullptr,
		(void**)&function);

	assert(rc == 0 && function != nullptr && "Failed to locate function or assembly");

	std::cout << "=== Mocha Bootstrap Init ===" << std::endl;
	function(&args);
}
