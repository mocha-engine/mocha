#include "edict.h"

#include <baseentity.h>
#include <globalvars.h>

void EntityManager::ForEach( std::function<void( std::shared_ptr<BaseEntity> entity )> func )
{
	for ( auto& e : m_entities )
	{
		func( e.second );
	}
}

void EntityManager::Startup() {}

void EntityManager::Shutdown() {}