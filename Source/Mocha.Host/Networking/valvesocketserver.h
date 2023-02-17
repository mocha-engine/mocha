#pragma once
#include <GameNetworkingSockets/steam/steamnetworkingsockets.h>
#include <Managed/managedcallback.h>
#include <Misc/defs.h>
#include <Misc/handlemap.h>
#include <Util/util.h>

class ValveSocketServer
{
private:
	HSteamListenSocket m_socket{};
	HSteamNetPollGroup m_pollGroup{};
	ISteamNetworkingSockets* m_interface{ nullptr };
	HandleMap<HSteamNetConnection> m_connections{};

	ManagedCallback m_connectedCallback{};
	
public:
	void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t* info );

	GENERATE_BINDINGS ValveSocketServer( int port );
	GENERATE_BINDINGS void SetConnectedCallback( Handle callbackHandle );
	GENERATE_BINDINGS void SendData( Handle clientHandle, UtilArray interopMessage );
	GENERATE_BINDINGS void PumpEvents();

	~ValveSocketServer();
};
