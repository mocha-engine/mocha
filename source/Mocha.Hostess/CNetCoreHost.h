#pragma once
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

using string_t = std::basic_string<char_t>;

#define STR(s) L ## s
#define CH(c) L ## c
#define DIR_SEPARATOR L'\\'

class CNetCoreHost
{
public:
	void CallFunction(string_t config_path, string_t dll_path, string_t dotnet_type, string_t dotnet_method);
private:
};

