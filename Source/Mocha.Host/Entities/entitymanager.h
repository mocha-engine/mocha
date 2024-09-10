#pragma once

#include <Entities/baseentity.h>
#include <Misc/handlemap.h>
#include <Misc/mathtypes.h>
#include <Misc/subsystem.h>
#include <Util/util.h>
#include <functional>
#include <memory>
#include <unordered_map>

class SceneGraph : HandleMap<SceneMesh>, ISubSystem
{
public:
	template <typename T>
	Handle AddMesh( T sceneMesh );

	template <typename T>
	std::shared_ptr<T> GetMesh( Handle meshHandle );

	void ForEach( std::function<void( std::shared_ptr<SceneMesh> mesh )> func );

	template <typename T>
	void ForEachSpecific( std::function<void( std::shared_ptr<T> mesh )> func );

	void For( std::function<void( Handle handle, std::shared_ptr<SceneMesh> mesh )> func );

	void Startup() override{};

	void Shutdown() override{};

	GENERATE_BINDINGS SceneMesh* GetMesh( uint32_t meshHandle )
	{
		return GetMesh<SceneMesh>( meshHandle ).get();
	}

	GENERATE_BINDINGS Handle CreateMesh()
	{
		SceneMesh* sceneMesh = new SceneMesh();
		return AddMesh( *sceneMesh );
	}
};

template <typename T>
inline Handle SceneGraph::AddMesh( T actor )
{
	return AddSpecific<T>( actor );
}

template <typename T>
inline std::shared_ptr<T> SceneGraph::GetMesh( Handle meshHandle )
{
	return GetSpecific<T>( meshHandle );
}

inline void SceneGraph::ForEach( std::function<void( std::shared_ptr<SceneMesh> mesh )> func )
{
	HandleMap<SceneMesh>::ForEach( func );
}

inline void SceneGraph::For( std::function<void( Handle handle, std::shared_ptr<SceneMesh> mesh )> func )
{
	HandleMap<SceneMesh>::For( func );
}

template <typename T>
inline void SceneGraph::ForEachSpecific( std::function<void( std::shared_ptr<T> mesh )> func )
{
	ForEach( [&]( std::shared_ptr<SceneMesh> mesh ) {
		// Can we cast to this?
		auto derivedEntity = std::dynamic_pointer_cast<T>( mesh );

		if ( derivedEntity == nullptr )
			return;

		func( derivedEntity );
	} );
}