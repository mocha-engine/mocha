#include "valvesocketclient.h"

void ValveSocketClient::OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t* info )
{
	spdlog::info( "ValveSocketClient::OnConnectionStatusChanged, new state: {}", info->m_info.m_eState );

	if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_Connected )
	{
		SteamNetConnectionInfo_t connectionInfo;
		m_interface->GetConnectionInfo( m_connection, &connectionInfo );

		char* addrBuf;
		addrBuf = ( char* )malloc( 48 );
		connectionInfo.m_addrRemote.ToString( addrBuf, 48, true );

		std::string addrString( addrBuf );
		spdlog::info( "Client: connected to {}", addrString );
	}
	else if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_ClosedByPeer ||
	          info->m_info.m_eState == k_ESteamNetworkingConnectionState_ProblemDetectedLocally )
	{
		ErrorMessage( "A connection has been actively rejected or closed by the remote host." );
		goto dump;
	}
	else if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_ProblemDetectedLocally )
	{
		ErrorMessage( "A problem was detected with the connection, and it has been closed by the local host." );
		goto dump;
	}

	return;

dump:
	// Dump error info into console
	spdlog::info( "{}: {}", info->m_info.m_eEndReason, info->m_info.m_szEndDebug );
	abort();
}

static ValveSocketClient* s_client;
static void SteamNetConnectionStatusChangedCallback( SteamNetConnectionStatusChangedCallback_t* pInfo )
{
	s_client->OnConnectionStatusChanged( pInfo );
}

ValveSocketClient::ValveSocketClient( const char* ip, int port )
{
	m_interface = SteamNetworkingSockets();

	SteamNetworkingIPAddr remoteAddress;
	remoteAddress.Clear();
	remoteAddress.ParseString( ip );
	remoteAddress.m_port = port;

	SteamNetworkingConfigValue_t options;
	options.SetPtr( k_ESteamNetworkingConfig_Callback_ConnectionStatusChanged, &SteamNetConnectionStatusChangedCallback );

	m_connection = m_interface->ConnectByIPAddress( remoteAddress, 1, &options );
	spdlog::info( "Client: attempting to connect to {} on port {}", ip, port );
}

ValveSocketClient::~ValveSocketClient()
{
	m_interface->CloseConnection( m_connection, 0, nullptr, true );
}

void ValveSocketClient::PumpEvents() {}

void ValveSocketClient::RunCallbacks()
{
	m_interface->RunCallbacks();
}