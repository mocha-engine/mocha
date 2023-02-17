#pragma once
#include <GameNetworkingSockets/steam/isteamnetworkingutils.h>
#include <GameNetworkingSockets/steam/steamnetworkingsockets.h>
#include <GameNetworkingSockets/steam/steamnetworkingtypes.h>
#include <Misc/defs.h>
#include <Misc/handlemap.h>
#include <Util/util.h>
#include <spdlog/spdlog.h>

inline static bool g_bNetworkingSocketsInitialized = false;

inline static void InitValveSockets()
{
	if ( g_bNetworkingSocketsInitialized )
		return;

	g_bNetworkingSocketsInitialized = true;

	SteamDatagramErrMsg errMsg;

	if ( !GameNetworkingSockets_Init( nullptr, errMsg ) )
	{
		std::stringstream ss;
		ss << "GameNetworkingSockets_Init failed.\n";
		ss << errMsg;

		ErrorMessage( ss.str() );
	}
}

//
// Bindings for GameNetworkingSockets
//

class ValveSocketServer
{
private:
	HSteamListenSocket m_socket = {};
	ISteamNetworkingSockets* m_interface;
	HandleMap<HSteamNetConnection> m_connections;

public:
	GENERATE_BINDINGS ValveSocketServer( const char* ip, int port )
	{
		InitValveSockets();
		m_interface = SteamNetworkingSockets();

		SteamNetworkingIPAddr localAddress;
		localAddress.Clear();
		localAddress.ParseString( ip );
		localAddress.m_port = port;

		m_socket = m_interface->CreateListenSocketIP( localAddress, 0, nullptr );

		spdlog::info( "Created ValveSocketServer on port {}", port );
	}

	GENERATE_BINDINGS void SendData( Handle clientHandle, UtilArray interopMessage )
	{
		std::shared_ptr<HSteamNetConnection> destination = m_connections.Get( clientHandle );

		std::vector<int32_t> message = interopMessage.GetData<int32_t>();

		m_interface->SendMessageToConnection(
		    *destination.get(), message.data(), sizeof( int32_t ) * 2, k_nSteamNetworkingSend_Reliable, nullptr );
	}

	~ValveSocketServer() { m_interface->CloseListenSocket( m_socket ); }
};

class ValveSocketClient
{
private:
	HSteamNetConnection m_connection = {};
	ISteamNetworkingSockets* m_interface;

public:
	GENERATE_BINDINGS ValveSocketClient( const char* ip, int port )
	{
		InitValveSockets();
		m_interface = SteamNetworkingSockets();

		SteamNetworkingIPAddr remoteAddress;
		remoteAddress.Clear();
		remoteAddress.ParseString( ip );
		remoteAddress.m_port = port;

		m_connection = m_interface->ConnectByIPAddress( remoteAddress, 0, nullptr );
	}

	~ValveSocketClient() { m_interface->CloseConnection( m_connection, 0, nullptr, true ); }
};