#pragma once
#include <GameNetworkingSockets/steam/isteamnetworkingutils.h>
#include <GameNetworkingSockets/steam/steamnetworkingsockets.h>
#include <GameNetworkingSockets/steam/steamnetworkingtypes.h>
#include <Managed/managedcallback.h>
#include <Misc/defs.h>
#include <Misc/handlemap.h>
#include <Util/util.h>
#include <spdlog/spdlog.h>

class ValveSocketClient
{
private:
	HSteamNetConnection m_connection = {};
	ISteamNetworkingSockets* m_interface;

public:
	void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t* info );

	GENERATE_BINDINGS ValveSocketClient( const char* ip, int port );

	~ValveSocketClient();
};