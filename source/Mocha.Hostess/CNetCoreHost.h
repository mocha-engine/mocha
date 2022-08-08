#pragma once

#include <Windows.h>
#include <assert.h>
#include <coreclr_delegates.h>
#include <hostfxr.h>
#include <iostream>
#include <nethost.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

using string_t = std::basic_string<char_t>;

#define STR( s ) L##s
#define CH( c ) L##c
#define DIR_SEPARATOR L'\\'

class CNetCoreHost
{
private:
	string_t mDllPath;
	string_t mConfigPath;

	load_assembly_and_get_function_pointer_fn mLoadAssemblyAndGetFunctionPointer;

public:
	CNetCoreHost( string_t config_path, string_t dll_path );

	void* FindFunction( string_t dotnet_type, string_t dotnet_method );
};
