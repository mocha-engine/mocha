#pragma once

#include <baseentity.h>
#include <functional>
#include <game/types.h>
#include <subsystem.h>
#include <unordered_map>

class EntityManager;

class EntityManager : public ISubSystem
{
private:
	// A map of entities, indexed by their handle index.
	std::unordered_map<uint32_t, std::shared_ptr<BaseEntity>> m_entities;

	// The current index to use when inserting a new entity into the map.
	uint32_t m_entIndex;

public:
	template <typename T>
	uint32_t AddEntity( T entity );

	template <typename T>
	T* GetEntity( uint32_t entityHandle );

	// Calls the specified function for each entity managed by this EntityManager.
	// The function should take a std::shared_ptr<BaseEntity> as its argument.
	void ForEach( std::function<void( std::shared_ptr<BaseEntity> entity )> func );

	// Called when the EntityManager is being started up.
	void Startup() override;

	// Called when the EntityManager is being shut down.
	void Shutdown() override;
};

template <typename T>
inline T* EntityManager::GetEntity( uint32_t entityHandle )
{
	return static_cast<T*>( m_entities[entityHandle].get() );
}

template <typename T>
inline uint32_t EntityManager::AddEntity( T entity )
{
	// Create a shared pointer to the entity.
	auto entityPtr = std::make_shared<T>( entity );

	// Add the entity to the map.
	m_entities[m_entIndex] = entityPtr;
	
	return m_entIndex++;
}
