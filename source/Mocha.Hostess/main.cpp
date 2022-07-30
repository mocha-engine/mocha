#include "CNetCoreHost.h"

int main()
{
	char_t host_path[MAX_PATH];
	auto size = GetCurrentDirectory(MAX_PATH, host_path);
	assert(size != 0);

	string_t root_path = host_path;
	string_t engine_dir = root_path + STR("\\");

	CNetCoreHost net_core_host;
	net_core_host.CallFunction(
		engine_dir + STR("Mocha.Engine.runtimeconfig.json"),
		engine_dir + STR("Mocha.Engine.dll"),
		STR("Mocha.Engine.Program, Mocha.Engine"),
		STR("HostedMain")
	);

	return 0;
}