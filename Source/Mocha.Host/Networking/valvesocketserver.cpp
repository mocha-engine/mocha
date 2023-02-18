#include "valvesocketserver.h"

#include <GameNetworkingSockets/steam/isteamnetworkingutils.h>
#include <GameNetworkingSockets/steam/steamnetworkingtypes.h>
#include <Misc/handlemap.h>
#include <memory>
#include <spdlog/spdlog.h>

void ValveSocketServer::OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t* info )
{
	spdlog::info( "ValveSocketServer::OnConnectionStatusChanged, new state: {}", info->m_info.m_eState );
	
	if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_Connected )
	{
		spdlog::info( "New client connected!" );

		m_connections.Add( info->m_hConn );
		m_connectedCallback.Invoke();
	}
}

static ValveSocketServer* s_server;
static void SteamNetConnectionStatusChangedCallback( SteamNetConnectionStatusChangedCallback_t* pInfo )
{
	s_server->OnConnectionStatusChanged( pInfo );
}

ValveSocketServer::ValveSocketServer( int port )
{
	m_interface = SteamNetworkingSockets();

	SteamNetworkingIPAddr localAddress;
	localAddress.Clear();
	localAddress.m_port = port;

	SteamNetworkingConfigValue_t options;
	options.SetPtr( k_ESteamNetworkingConfig_Callback_ConnectionStatusChanged, &SteamNetConnectionStatusChangedCallback );

	m_socket = m_interface->CreateListenSocketIP( localAddress, 1, &options );

	spdlog::info( "Created ValveSocketServer on port {}", port );
}

void ValveSocketServer::SetConnectedCallback( Handle callbackHandle )
{
	spdlog::info( "Registered connected callback" );
	m_connectedCallback = ManagedCallback( callbackHandle );
}

void ValveSocketServer::SendData( Handle clientHandle, UtilArray interopMessage )
{
	std::shared_ptr<HSteamNetConnection> destination = m_connections.Get( clientHandle );

	std::vector<int32_t> message = interopMessage.GetData<int32_t>();

	m_interface->SendMessageToConnection(
	    *destination.get(), message.data(), sizeof( int32_t ) * 2, k_nSteamNetworkingSend_Reliable, nullptr );
}

void ValveSocketServer::PumpEvents()
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

	//
	// Run callbacks
	//
	m_interface->RunCallbacks();
}

ValveSocketServer::~ValveSocketServer()
{
	m_interface->CloseListenSocket( m_socket );
}