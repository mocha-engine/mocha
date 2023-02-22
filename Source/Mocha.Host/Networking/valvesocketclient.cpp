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
		spdlog::info( "Client: connected to '{}'", addrString );

		m_isConnected = true;

		free( addrBuf );
	}
	else if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_ClosedByPeer ||
	          info->m_info.m_eState == k_ESteamNetworkingConnectionState_ProblemDetectedLocally )
	{
		// Dump error info into console
		spdlog::info( "{}: {}", info->m_info.m_eEndReason, info->m_info.m_szEndDebug );

		// Show user-facing error
		if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_ClosedByPeer )
			ErrorMessage( "A connection has been actively rejected or closed by the remote host." );
		else if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_ProblemDetectedLocally )
			ErrorMessage( "A problem was detected with the connection, and it has been closed by the local host." );

		m_isConnected = false;

		abort();
	}
	else if ( info->m_info.m_eState == k_ESteamNetworkingConnectionState_None ||
	          info->m_info.m_eState == k_ESteamNetworkingConnectionState_Dead )
	{
		spdlog::info( "Client: disconnected" );
		m_isConnected = false;
	}
}

static ValveSocketClient* s_client;
static void SteamNetConnectionStatusChangedCallback( SteamNetConnectionStatusChangedCallback_t* pInfo )
{
	s_client->OnConnectionStatusChanged( pInfo );
}

ValveSocketClient::ValveSocketClient( const char* ip, int port )
{
	m_interface = SteamNetworkingSockets();
	s_client = this;

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

void ValveSocketClient::PumpEvents()
{
	ISteamNetworkingMessage* incomingMsg{ nullptr };
	int messageCount = m_interface->ReceiveMessagesOnConnection( m_connection, &incomingMsg, 1 );

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
	receivedMessage.connectionHandle = ( void* )m_connection;
	receivedMessage.size = incomingMsg->m_cbSize;
	receivedMessage.data = ( void* )data;

	m_dataReceivedCallback.Invoke( ( void* )&receivedMessage );
}

void ValveSocketClient::SetDataReceivedCallback( Handle callbackHandle )
{
	spdlog::info( "Registered data received callback" );
	m_dataReceivedCallback = callbackHandle;
}

void ValveSocketClient::RunCallbacks()
{
	m_interface->RunCallbacks();
}

void ValveSocketClient::SendData( UtilArray interopData )
{
	if ( !m_isConnected )
		return;

	m_interface->SendMessageToConnection(
	    m_connection, interopData.data, interopData.size, k_nSteamNetworkingSend_Reliable, nullptr );
}