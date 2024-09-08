#pragma once
#include <GameNetworkingSockets/steam/isteamnetworkingutils.h>
#include <GameNetworkingSockets/steam/steamnetworkingsockets.h>
#include <GameNetworkingSockets/steam/steamnetworkingtypes.h>
#include <Managed/managedcallback.h>
#include <Misc/defs.h>
#include <Misc/handlemap.h>
#include <Networking/valvesocketreceivedmessage.h>
#include <Util/util.h>
#include <spdlog/spdlog.h>

class ValveSocketClient
{
private:
	HSteamNetConnection m_connection = {};
	ISteamNetworkingSockets* m_interface;
	bool m_isConnected{ false };

	ManagedCallback m_dataReceivedCallback{};

public:
	void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t* info );

	GENERATE_BINDINGS ValveSocketClient( const char* ip, int port );
	
	GENERATE_BINDINGS void SetDataReceivedCallback( Handle callbackHandle );
	
	GENERATE_BINDINGS void PumpEvents();
	GENERATE_BINDINGS void RunCallbacks();
	GENERATE_BINDINGS void SendData( UtilArray interopData );

	~ValveSocketClient();
};