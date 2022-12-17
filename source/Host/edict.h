#pragma once

#include <baseentity.h>
#include <functional>
#include <game_types.h>
#include <handlemap.h>
#include <memory>
#include <subsystem.h>
#include <unordered_map>

class EntityManager : HandleMap<BaseEntity>, ISubSystem
{
public:
	template <typename T>
	Handle AddEntity( T entity );

	template <typename T>
	std::shared_ptr<T> GetEntity( Handle entityHandle );

	void ForEach( std::function<void( std::shared_ptr<BaseEntity> entity )> func );

	template <typename T>
	void ForEachSpecific( std::function<void( std::shared_ptr<T> entity )> func );
	
	void For( std::function<void( Handle handle, std::shared_ptr<BaseEntity> entity )> func );

	void Startup() override;

	void Shutdown() override;
};

template <typename T>
inline Handle EntityManager::AddEntity( T entity )
{
	return AddSpecific<T>( entity );
}

template <typename T>
inline std::shared_ptr<T> EntityManager::GetEntity( Handle entityHandle )
{
	return GetSpecific<T>( entityHandle );
}

inline void EntityManager::ForEach( std::function<void( std::shared_ptr<BaseEntity> entity )> func )
{
	HandleMap<BaseEntity>::ForEach( func );
}

inline void EntityManager::For( std::function<void( Handle handle, std::shared_ptr<BaseEntity> entity )> func )
{
	HandleMap<BaseEntity>::For( func );
}

template <typename T>
inline void EntityManager::ForEachSpecific( std::function<void( std::shared_ptr<T> entity )> func )
{
	ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
		// Can we cast to this?
		auto derivedEntity = std::dynamic_pointer_cast<T>( entity );

		if ( derivedEntity == nullptr )
			return;

		func( derivedEntity );
	} );
}