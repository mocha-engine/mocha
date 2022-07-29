#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>
#include <iostream>
#include <nethost.h>
#include <coreclr_delegates.h>
#include <hostfxr.h>
#include <Windows.h>

#define STR(s) L ## s
#define CH(c) L ## c
#define DIR_SEPARATOR L'\\'

using string_t = std::basic_string<char_t>;

namespace
{
	hostfxr_initialize_for_runtime_config_fn init_fptr;
	hostfxr_get_runtime_delegate_fn get_delegate_fptr;
	hostfxr_close_fn close_fptr;

	bool load_hostfxr();
	load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const char_t* assembly);
}

int __cdecl wmain(int argc, wchar_t* argv[])
{
	char_t host_path[MAX_PATH];
	auto size = GetCurrentDirectory(MAX_PATH, host_path);
	assert(size != 0);

	string_t root_path = host_path;
	auto pos = root_path.find_last_of(DIR_SEPARATOR);
	assert(pos != string_t::npos);
	root_path = root_path.substr(0, pos + 1);

	// Load HostFxr and get exported hosting functions
	if (!load_hostfxr())
	{
		assert(false && "Failure: load_hostfxr()");
		return EXIT_FAILURE;
	}

	// Initialize and start the .NET Core runtime
	const string_t config_path = root_path + STR("\\source\\Mocha.Script\\bin\\Debug\\net6.0\\Mocha.Script.runtimeconfig.json");
	load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer = nullptr;
	load_assembly_and_get_function_pointer = get_dotnet_load_assembly(config_path.c_str());
	assert(load_assembly_and_get_function_pointer != nullptr && "Failure: get_dotnet_load_assembly()");

	// Load managed assembly and get function pointer to a managed method
	const string_t dotnetlib_path = root_path + STR("\\source\\Mocha.Script\\bin\\Debug\\net6.0\\Mocha.Script.dll");
	const char_t* dotnet_type = STR("Mocha.Script.Program, Mocha.Script");
	const char_t* dotnet_type_method = STR("Hello");

	// Function pointer to managed delegate with non-default signature
	typedef void (CORECLR_DELEGATE_CALLTYPE* custom_entry_point_fn)(int number);
	custom_entry_point_fn hello_world_fn = nullptr;
	int rc = load_assembly_and_get_function_pointer(
		dotnetlib_path.c_str(),
		dotnet_type,
		STR("CustomEntryPointUnmanaged") /*method_name*/,
		UNMANAGEDCALLERSONLY_METHOD,
		nullptr,
		(void**)&hello_world_fn);

	assert(rc == 0 && hello_world_fn != nullptr && "Failure: load_assembly_and_get_function_pointer()");

	// Call managed function with a parameter
	hello_world_fn(42);

	return EXIT_SUCCESS;
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