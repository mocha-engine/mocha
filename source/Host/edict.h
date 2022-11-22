#pragma once

#include <functional>
#include <game/types.h>
#include <map>
#include <subsystem.h>
#include <baseentity.h>


class EDict : ISubSystem
{
private:
	std::unordered_map<uint32_t, std::shared_ptr<BaseEntity>> m_entities;
	uint32_t m_entIndex;

public:
	BaseEntity* GetEntity( uint32_t entityHandle );

	template <typename T>
	uint32_t AddEntity( T entity );

	void ForEach( std::function<void( std::shared_ptr<BaseEntity> entity )> func );

	void Startup();
	void Shutdown();
};

template <typename T>
inline uint32_t EDict::AddEntity( T entity )
{
	m_entities.emplace( ++m_entIndex, std::make_shared<T>( entity ) );
	return m_entIndex;
}
