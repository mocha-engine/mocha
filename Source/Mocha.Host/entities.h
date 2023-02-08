#pragma once
#include <baseentity.h>
#include <entitymanager.h>
#include <globalvars.h>
#include <modelentity.h>

// TODO: I hate this
namespace Entities
{
	GENERATE_BINDINGS inline uint32_t CreateBaseEntity()
	{
		// TODO: Derive root based on current context / realm
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		BaseEntity baseEntity = {};
		baseEntity.AddFlag( ENTITY_MANAGED );
		baseEntity.m_type = "BaseEntity";

		return entityDictionary->AddEntity<BaseEntity>( baseEntity );
	}

	GENERATE_BINDINGS inline uint32_t CreateModelEntity()
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		ModelEntity modelEntity = {};
		modelEntity.AddFlag( ENTITY_MANAGED );
		modelEntity.AddFlag( ENTITY_RENDERABLE );
		modelEntity.m_type = "ModelEntity";

		return entityDictionary->AddEntity<ModelEntity>( modelEntity );
	}

	GENERATE_BINDINGS inline void SetViewmodel( uint32_t handle, bool isViewmodel )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->m_transform;

		if ( isViewmodel )
			entity->AddFlag( ENTITY_VIEWMODEL );
		else
			entity->RemoveFlag( ENTITY_VIEWMODEL );
	}

	GENERATE_BINDINGS inline void SetUI( uint32_t handle, bool isUI )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->m_transform;

		if ( isUI )
			entity->AddFlag( ENTITY_UI );
		else
			entity->RemoveFlag( ENTITY_UI );
	}

	GENERATE_BINDINGS inline void SetPosition( uint32_t handle, Vector3 position )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->m_transform;

		transform.position = position;

		entity->m_transform = transform;
	}

	GENERATE_BINDINGS inline void SetRotation( uint32_t handle, Quaternion rotation )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->m_transform;

		transform.rotation = rotation;

		entity->m_transform = transform;
	}

	GENERATE_BINDINGS inline void SetScale( uint32_t handle, Vector3 scale )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->m_transform;

		transform.scale = scale;

		entity->m_transform = transform;
	}

	GENERATE_BINDINGS inline void SetName( uint32_t handle, const char* name )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<BaseEntity>( handle );

		entity->m_name = name;
	}

	GENERATE_BINDINGS inline Vector3 GetPosition( uint32_t handle )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->m_transform;

		return transform.position;
	}

	GENERATE_BINDINGS inline Quaternion GetRotation( uint32_t handle )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->m_transform;

		return transform.rotation;
	}

	GENERATE_BINDINGS inline Vector3 GetScale( uint32_t handle )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<BaseEntity>( handle );
		auto transform = entity->m_transform;

		return transform.scale;
	}

	GENERATE_BINDINGS inline const char* GetName( uint32_t handle )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<BaseEntity>( handle );
		return entity->m_name.c_str();
	}

	GENERATE_BINDINGS inline void SetModel( uint32_t handle, Model model )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetModel( model );
	}

	GENERATE_BINDINGS inline void SetCameraPosition( Vector3 position )
	{
		auto& root = ClientRoot::GetInstance();

		root.m_cameraPos = position;
	}

	GENERATE_BINDINGS inline Vector3 GetCameraPosition()
	{
		auto& root = ClientRoot::GetInstance();

		return root.m_cameraPos;
	}

	GENERATE_BINDINGS inline void SetCameraRotation( Quaternion rotation )
	{
		auto& root = ClientRoot::GetInstance();

		root.m_cameraRot = rotation;
	}

	GENERATE_BINDINGS inline Quaternion GetCameraRotation()
	{
		auto& root = ClientRoot::GetInstance();

		return root.m_cameraRot;
	}

	GENERATE_BINDINGS inline void SetCameraFieldOfView( float fov )
	{
		auto& root = ClientRoot::GetInstance();

		root.m_cameraFov = fov;
	}

	GENERATE_BINDINGS inline float GetCameraFieldOfView()
	{
		auto& root = ClientRoot::GetInstance();

		return root.m_cameraFov;
	}

	GENERATE_BINDINGS inline void SetCameraZNear( float znear )
	{
		auto& root = ClientRoot::GetInstance();

		root.m_cameraZNear = znear;
	}

	GENERATE_BINDINGS inline float GetCameraZNear()
	{
		auto& root = ClientRoot::GetInstance();

		return root.m_cameraZNear;
	}

	GENERATE_BINDINGS inline void SetCameraZFar( float zfar )
	{
		auto& root = ClientRoot::GetInstance();

		root.m_cameraZFar = zfar;
	}

	GENERATE_BINDINGS inline float GetCameraZFar()
	{
		auto& root = ClientRoot::GetInstance();

		return root.m_cameraZFar;
	}

	GENERATE_BINDINGS inline void SetCubePhysics( uint32_t handle, Vector3 bounds, bool isStatic )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetCubePhysics( bounds, isStatic );
	}

	GENERATE_BINDINGS inline void SetSpherePhysics( uint32_t handle, float radius, bool isStatic )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetSpherePhysics( radius, isStatic );
	}

	GENERATE_BINDINGS inline void SetMeshPhysics( uint32_t handle, int vertexSize, void* vertexData )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
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

	GENERATE_BINDINGS inline void SetVelocity( uint32_t handle, Vector3 velocity )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetVelocity( velocity );
	}

	GENERATE_BINDINGS inline Vector3 GetVelocity( uint32_t handle )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return {};
		}

		return entity->GetVelocity();
	}

	GENERATE_BINDINGS inline float GetMass( uint32_t handle )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return 0.0f;
		}

		return entity->GetMass();
	}

	GENERATE_BINDINGS inline float GetFriction( uint32_t handle )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return 0.0f;
		}

		return entity->GetFriction();
	}

	GENERATE_BINDINGS inline float GetRestitution( uint32_t handle )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return 0.0f;
		}

		return entity->GetRestitution();
	}

	GENERATE_BINDINGS inline void SetMass( uint32_t handle, float mass )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetMass( mass );
	}

	GENERATE_BINDINGS inline void SetFriction( uint32_t handle, float friction )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetFriction( friction );
	}

	GENERATE_BINDINGS inline void SetRestitution( uint32_t handle, float restitution )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetRestitution( restitution );
	}

	GENERATE_BINDINGS inline bool GetIgnoreRigidbodyPosition( uint32_t handle )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return false;
		}

		return entity->GetIgnoreRigidbodyPosition();
	}

	GENERATE_BINDINGS inline bool GetIgnoreRigidbodyRotation( uint32_t handle )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return false;
		}

		return entity->GetIgnoreRigidbodyRotation();
	}

	GENERATE_BINDINGS inline void SetIgnoreRigidbodyPosition( uint32_t handle, bool ignore )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;

		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetIgnoreRigidbodyPosition( ignore );
	}

	GENERATE_BINDINGS inline void SetIgnoreRigidbodyRotation( uint32_t handle, bool ignore )
	{
		auto& root = ClientRoot::GetInstance();
		auto* entityDictionary = root.m_entityManager;
		
		auto entity = entityDictionary->GetEntity<ModelEntity>( handle );
		if ( entity == nullptr )
		{
			spdlog::error( "Couldn't cast {} to ModelEntity", handle );
			return;
		}

		entity->SetIgnoreRigidbodyRotation( ignore );
	}
} // namespace Entities