#pragma once
#include <GameNetworkingSockets/steam/steamnetworkingsockets.h>
#include <Managed/managedcallback.h>
#include <Misc/defs.h>
#include <Misc/handlemap.h>
#include <Util/util.h>

struct ValveSocketReceivedMessage
{
	void* connectionHandle;
	
	int size;
	void* data;
};

class ValveSocketServer
{
private:
	HSteamListenSocket m_socket{};
	HSteamNetPollGroup m_pollGroup{};
	ISteamNetworkingSockets* m_interface{ nullptr };
	HandleMap<HSteamNetConnection> m_connections{};

	// TODO: remove
	ManagedCallback m_connectedCallback{};

	ManagedCallback m_clientConnectedCallback{};
	ManagedCallback m_clientDisconnectedCallback{};
	ManagedCallback m_dataReceivedCallback{};

public:
	void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t* info );

	GENERATE_BINDINGS ValveSocketServer( int port );

	GENERATE_BINDINGS void SetClientConnectedCallback( Handle callbackHandle );
	GENERATE_BINDINGS void SetClientDisconnectedCallback( Handle callbackHandle );
	GENERATE_BINDINGS void SetDataReceivedCallback( Handle callbackHandle );

	GENERATE_BINDINGS void SendData( Handle clientHandle, UtilArray interopMessage );
	GENERATE_BINDINGS void PumpEvents();
	GENERATE_BINDINGS void RunCallbacks();

	GENERATE_BINDINGS void Disconnect( Handle clientHandle ); 
	GENERATE_BINDINGS const char* GetRemoteAddress( Handle clientHandle );

	~ValveSocketServer();
};
