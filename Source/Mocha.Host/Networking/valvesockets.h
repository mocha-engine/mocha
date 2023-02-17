#pragma once
#include <GameNetworkingSockets/steam/isteamnetworkingutils.h>
#include <GameNetworkingSockets/steam/steamnetworkingsockets.h>
#include <GameNetworkingSockets/steam/steamnetworkingtypes.h>
#include <Managed/managedcallback.h>
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
	HSteamListenSocket m_socket{};
	HSteamNetPollGroup m_pollGroup{};
	ISteamNetworkingSockets* m_interface{ nullptr };
	HandleMap<HSteamNetConnection> m_connections{};

	ManagedCallback m_connectedCallback{};

public:
	void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t* info )
	{
		if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_Connected )
		{
			m_connections.Add( info->m_hConn );
			m_connectedCallback.Invoke();
		}
	}

	GENERATE_BINDINGS ValveSocketServer( const char* ip, int port )
	{
		InitValveSockets();
		m_interface = SteamNetworkingSockets();

		SteamNetworkingIPAddr localAddress;
		localAddress.Clear();
		localAddress.ParseString( ip );
		localAddress.m_port = port;

		SteamNetworkingConfigValue_t options;
		auto callback = [this]( SteamNetConnectionStatusChangedCallback_t* info ) { OnConnectionStatusChanged( info ); };
		options.SetPtr( k_ESteamNetworkingConfig_Callback_ConnectionStatusChanged, static_cast<void*>( &callback ) );

		m_socket = m_interface->CreateListenSocketIP( localAddress, 0, nullptr );

		spdlog::info( "Created ValveSocketServer on port {}", port );
	}

	GENERATE_BINDINGS void SetConnectedCallback( Handle callbackHandle )
	{
		m_connectedCallback = ManagedCallback( callbackHandle );
	}

	GENERATE_BINDINGS void SendData( Handle clientHandle, UtilArray interopMessage )
	{
		std::shared_ptr<HSteamNetConnection> destination = m_connections.Get( clientHandle );

		std::vector<int32_t> message = interopMessage.GetData<int32_t>();

		m_interface->SendMessageToConnection(
		    *destination.get(), message.data(), sizeof( int32_t ) * 2, k_nSteamNetworkingSend_Reliable, nullptr );
	}

	/// <summary>
	/// Call this every tick.
	/// </summary>
	GENERATE_BINDINGS void PumpEvents()
	{
		ISteamNetworkingMessage* incomingMsg{ nullptr };
		int messageCount = m_interface->ReceiveMessagesOnPollGroup( m_pollGroup, &incomingMsg, 1 );

		if ( messageCount == 0 )
			return;

		if ( messageCount < 0 )
			ErrorMessage( "nice one dickhead" );

		char* ptrData = ( char* )incomingMsg->m_pData;
		UtilArray data{};
		data.count = incomingMsg->m_cbSize;
		data.data = ptrData;
		data.size = data.count * sizeof( char );

		incomingMsg->Release();
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