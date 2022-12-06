#pragma once
#include <baseentity.h>
#include <edict.h>
#include <globalvars.h>
#include <modelentity.h>
#include <game/camera.h>
#include <game/managedmodel.h>

//@InteropGen generate class
namespace Entities
{
	inline uint32_t CreateBaseEntity()
	{
		BaseEntity baseEntity = {};
		baseEntity.AddFlag( ENTITY_MANAGED );
		baseEntity.SetType( "BaseEntity" );

		spdlog::trace( "Created base entity" );

		return g_entityDictionary->AddEntity<BaseEntity>( baseEntity );
	}

	inline uint32_t CreateModelEntity()
	{
		ModelEntity modelEntity = {};
		modelEntity.AddFlag( ENTITY_MANAGED );
		modelEntity.AddFlag( ENTITY_RENDERABLE );
		modelEntity.SetType( "ModelEntity" );

		spdlog::trace( "Created model entity" );

		return g_entityDictionary->AddEntity<ModelEntity>( modelEntity );
	}

	inline void SetPosition( uint32_t handle, Vector3 position )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->GetTransform();

		transform.position = position;

		entity->SetTransform( transform );
	}

	inline void SetRotation( uint32_t handle, Quaternion rotation )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->GetTransform();

		transform.rotation = rotation;

		entity->SetTransform( transform );
	}

	inline void SetScale( uint32_t handle, Vector3 scale )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->GetTransform();

		transform.scale = scale;

		entity->SetTransform( transform );
	}

	inline void SetName( uint32_t handle, std::string name )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );

		entity->SetName( name );
	}

	inline Vector3 GetPosition( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->GetTransform();

		return transform.position;
	}

	inline Quaternion GetRotation( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->GetTransform();

		return transform.rotation;
	}

	inline Vector3 GetScale( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->GetTransform();

		return transform.scale;
	}

	inline std::string GetName( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );
		return entity->GetName();
	}

	inline void SetModel( uint32_t handle, ManagedModel* model )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetModel( model->GetModel() );
	}

	inline void SetCameraPosition(Vector3 position)
	{
		g_cameraPos = position;
	}
} // namespace Entities