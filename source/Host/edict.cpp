#include "edict.h"

#include <baseentity.h>

BaseEntity* EDict::GetEntity( uint32_t entityHandle )
{
	return m_entities[entityHandle].get();
}

void EDict::ForEach( std::function<void( std::shared_ptr<BaseEntity> entity )> func )
{
	for ( auto& e : m_entities )
	{
		func( e.second );
	}
}

void EDict::Startup() {}

void EDict::Shutdown() {}