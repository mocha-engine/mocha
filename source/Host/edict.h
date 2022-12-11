#pragma once

#include <baseentity.h>
#include <functional>
#include <game/types.h>
#include <handlemap.h>
#include <memory>
#include <subsystem.h>
#include <unordered_map>

class EntityManager : HandleMap<BaseEntity>, ISubSystem
{
public:
	template <typename T>
	uint32_t AddEntity( T entity );

	template <typename T>
	std::shared_ptr<T> GetEntity( uint32_t entityHandle );

	// Calls the specified function for each entity managed by this EntityManager.
	// The function should take a std::shared_ptr<BaseEntity> as its argument.
	void ForEach( std::function<void( std::shared_ptr<BaseEntity> entity )> func );

	// Called when the EntityManager is being started up.
	void Startup() override;

	// Called when the EntityManager is being shut down.
	void Shutdown() override;
};

template <typename T>
inline uint32_t EntityManager::AddEntity( T entity )
{
	return AddSpecific<T>( entity );
}

template <typename T>
inline std::shared_ptr<T> EntityManager::GetEntity( uint32_t entityHandle )
{
	return GetSpecific<T>( entityHandle );
}

inline void EntityManager::ForEach( std::function<void( std::shared_ptr<BaseEntity> entity )> func )
{
	HandleMap<BaseEntity>::ForEach( func );
}