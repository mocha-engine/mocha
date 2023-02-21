#include "valvesocketserver.h"

#include <GameNetworkingSockets/steam/isteamnetworkingutils.h>
#include <GameNetworkingSockets/steam/steamnetworkingtypes.h>
#include <Misc/handlemap.h>
#include <memory>
#include <spdlog/spdlog.h>

void ValveSocketServer::OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t* info )
{
	spdlog::info( "ValveSocketServer::OnConnectionStatusChanged, new state: {}", info->m_info.m_eState );

	if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_Connecting )
	{
		// TODO: Make sure this client isn't already connected
		if ( m_connections.Find( info->m_hConn ) != HANDLE_INVALID )
		{
			// Get IP address so we can log it
			char* addrBuf;
			addrBuf = ( char* )malloc( 48 );
			info->m_info.m_addrRemote.ToString( addrBuf, 48, true );

			std::string addrString( addrBuf );

			spdlog::error( "'{}' tried to connect, but we already had them in the list of connected clients?", addrString );

			free( addrBuf );
			return;
		}

		// Accept connection
		m_interface->AcceptConnection( info->m_hConn );

		// Assign poll group
		m_interface->SetConnectionPollGroup( info->m_hConn, m_pollGroup );
	}
	else if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_Connected )
	{
		spdlog::info( "New client connected!" );

		Handle clientHandle = m_connections.Add( info->m_hConn );

		// Do something with the client now that they're connected
		m_clientConnectedCallback.Invoke( ( void* )clientHandle );
	}
	else if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_ClosedByPeer )
	{
		spdlog::info( "Client disconnected!" );

		Handle clientHandle = m_connections.Find( info->m_hConn );

		// Do something with the client before we remove all traces of the connection...
		m_clientDisconnectedCallback.Invoke( ( void* )clientHandle );

		m_connections.Remove( info->m_hConn );
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
	s_server = this;

	SteamNetworkingIPAddr localAddress;
	localAddress.Clear();
	localAddress.m_port = port;

	SteamNetworkingConfigValue_t options;
	options.SetPtr( k_ESteamNetworkingConfig_Callback_ConnectionStatusChanged, &SteamNetConnectionStatusChangedCallback );

	m_socket = m_interface->CreateListenSocketIP( localAddress, 1, &options );
	m_pollGroup = m_interface->CreatePollGroup();

	spdlog::info( "Created ValveSocketServer on port {}", port );
}

void ValveSocketServer::SetClientConnectedCallback( Handle callbackHandle )
{
	spdlog::info( "Registered client connected callback" );
	m_clientConnectedCallback = callbackHandle;
}

void ValveSocketServer::SetClientDisconnectedCallback( Handle callbackHandle )
{
	spdlog::info( "Registered client disconnected callback" );
	m_clientDisconnectedCallback = callbackHandle;
}

void ValveSocketServer::SetDataReceivedCallback( Handle callbackHandle )
{
	spdlog::info( "Registered data received callback" );
	m_dataReceivedCallback = callbackHandle;
}

void ValveSocketServer::SendData( Handle clientHandle, UtilArray interopMessage )
{
	std::shared_ptr<HSteamNetConnection> destination = m_connections.Get( clientHandle );

	std::vector<char> message = interopMessage.GetData<char>();

	m_interface->SendMessageToConnection(
	    *destination.get(), message.data(), message.size(), k_nSteamNetworkingSend_Reliable, nullptr );
}

void ValveSocketServer::PumpEvents()
{
	ISteamNetworkingMessage* incomingMsg{ nullptr };
	int messageCount = m_interface->ReceiveMessagesOnPollGroup( m_pollGroup, &incomingMsg, 1 );

	if ( messageCount == 0 )
		return;

	if ( messageCount < 0 )
	{
		std::stringstream ss;
		ss << "Expected message count 0 or 1, got ";
		ss << messageCount;
		ss << " instead.";
		ErrorMessage( ss.str() );
		abort();
	}

	char* ptrData = ( char* )incomingMsg->m_pData;

	// Convert to string
	const char* data = ( const char* )malloc( incomingMsg->m_cbSize );
	memcpy_s( ( void* )data, incomingMsg->m_cbSize, ptrData, incomingMsg->m_cbSize );

	incomingMsg->Release();

	ValveSocketReceivedMessage receivedMessage{};
	receivedMessage.connectionHandle = ( void* )m_connections.Find( incomingMsg->m_conn );
	receivedMessage.size = incomingMsg->m_cbSize;
	receivedMessage.data = ( void* )data;

	m_dataReceivedCallback.Invoke( ( void* )&receivedMessage );
}

void ValveSocketServer::RunCallbacks()
{
	m_interface->RunCallbacks();
}

ValveSocketServer::~ValveSocketServer()
{
	m_interface->CloseListenSocket( m_socket );
}

const char* ValveSocketServer::GetRemoteAddress( Handle clientHandle )
{
	SteamNetConnectionInfo_t connectionInfo;
	m_interface->GetConnectionInfo( *m_connections.Get( clientHandle ).get(), &connectionInfo );

	char* addrBuf;
	addrBuf = ( char* )malloc( 48 );
	connectionInfo.m_addrRemote.ToString( addrBuf, 48, true );

	return addrBuf;
}

void ValveSocketServer::Disconnect( Handle clientHandle )
{
	HSteamNetConnection connection = *m_connections.Get( clientHandle ).get();
	m_interface->CloseConnection( connection, k_ESteamNetConnectionEnd_App_Generic, "Disconnecting", true );
}