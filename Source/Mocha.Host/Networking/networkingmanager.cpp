#include "networkingmanager.h"

#include <GameNetworkingSockets/steam/steamnetworkingsockets.h>
#include <GameNetworkingSockets/steam/steamnetworkingtypes.h>
#include <Misc/defs.h>
#include <sstream>

void NetworkingManager::Startup()
{
	SteamDatagramErrMsg errMsg;

	if ( !GameNetworkingSockets_Init( nullptr, errMsg ) )
	{
		std::stringstream ss;
		ss << "GameNetworkingSockets_Init failed.\n";
		ss << errMsg;

		ErrorMessage( ss.str() );
	}
}

void NetworkingManager::Shutdown()
{
	GameNetworkingSockets_Kill();
}
