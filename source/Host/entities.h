#pragma once
#include <baseentity.h>
#include <camera.h>
#include <edict.h>
#include <globalvars.h>
#include <modelentity.h>

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

	inline void SetViewmodel( uint32_t handle, bool isViewmodel )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->GetTransform();

		if ( isViewmodel )
			entity->AddFlag( ENTITY_VIEWMODEL );
		else
			entity->RemoveFlag( ENTITY_VIEWMODEL );
	}

	inline void SetUI( uint32_t handle, bool isUI )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->GetTransform();

		if ( isUI )
			entity->AddFlag( ENTITY_UI );
		else
			entity->RemoveFlag( ENTITY_UI );
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

	inline void SetName( uint32_t handle, const char* name )
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

	inline const char* GetName( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<BaseEntity>( handle );
		return entity->GetName();
	}

	inline void SetModel( uint32_t handle, Model model )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetModel( model );
	}

	inline void SetCameraPosition( Vector3 position )
	{
		g_cameraPos = position;
	}

	inline Vector3 GetCameraPosition()
	{
		return g_cameraPos;
	}

	inline void SetCameraRotation( Quaternion rotation )
	{
		g_cameraRot = rotation;
	}

	inline Quaternion GetCameraRotation()
	{
		return g_cameraRot;
	}

	inline void SetCameraFieldOfView( float fov )
	{
		g_cameraFov = fov;
	}

	inline float GetCameraFieldOfView()
	{
		return g_cameraFov;
	}

	inline void SetCameraZNear( float znear )
	{
		g_cameraZNear = znear;
	}

	inline float GetCameraZNear()
	{
		return g_cameraZNear;
	}

	inline void SetCameraZFar( float zfar )
	{
		g_cameraZFar = zfar;
	}

	inline float GetCameraZFar()
	{
		return g_cameraZFar;
	}

	inline float GetCurrentTime()
	{
		return g_curTime;
	}

	inline float GetDeltaTime()
	{
		return g_frameTime;
	}

	inline void SetCubePhysics( uint32_t handle, Vector3 bounds, bool isStatic )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetCubePhysics( bounds, isStatic );
	}

	inline void SetSpherePhysics( uint32_t handle, float radius, bool isStatic )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetSpherePhysics( radius, isStatic );
	}

	inline void SetMeshPhysics( uint32_t handle, int vertexSize, void* vertexData )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		// Convert data to points
		Vector3* vertices = ( Vector3* )vertexData;
		size_t vertCount = vertexSize / sizeof( Vector3 );

		std::vector<Vector3> vertexList = {};
		vertexList.resize( vertCount );
		vertexList.insert( vertexList.begin(), vertices, vertices + vertCount );

		entity->SetMeshPhysics( vertexList );
	}

	inline void SetVelocity( uint32_t handle, Vector3 velocity )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetVelocity( velocity );
	}

	inline Vector3 GetVelocity( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return {};
		}

		return entity->GetVelocity();
	}

	inline float GetMass( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return 0.0f;
		}

		return entity->GetMass();
	}

	inline float GetFriction( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return 0.0f;
		}

		return entity->GetFriction();
	}

	inline float GetRestitution( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return 0.0f;
		}

		return entity->GetRestitution();
	}

	inline void SetMass( uint32_t handle, float mass )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetMass( mass );
	}

	inline void SetFriction( uint32_t handle, float friction )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetFriction( friction );
	}

	inline void SetRestitution( uint32_t handle, float restitution )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetRestitution( restitution );
	}

	inline bool GetIgnoreRigidbodyPosition( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return false;
		}

		return entity->GetIgnoreRigidbodyPosition();
	}

	inline bool GetIgnoreRigidbodyRotation( uint32_t handle )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return false;
		}

		return entity->GetIgnoreRigidbodyRotation();
	}

	inline void SetIgnoreRigidbodyPosition( uint32_t handle, bool ignore )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetIgnoreRigidbodyPosition( ignore );
	}

	inline void SetIgnoreRigidbodyRotation( uint32_t handle, bool ignore )
	{
		auto entity = g_entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetIgnoreRigidbodyRotation( ignore );
	}
} // namespace Entities