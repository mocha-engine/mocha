#pragma once
#include <baseentity.h>
#include <cvarmanager.h>
#include <defs.h>
#include <entitymanager.h>
#include <globalvars.h>
#include <modelentity.h>
#include <projectmanager.h>
#include <projectmanifest.h>

namespace Engine
{
	GENERATE_BINDINGS inline void Quit()
	{
		FindInstance().Quit();
	}

	GENERATE_BINDINGS inline int GetCurrentTick()
	{
		return FindInstance().m_curTick;
	}

	GENERATE_BINDINGS inline float GetFrameDeltaTime()
	{
		return FindInstance().m_frameDeltaTime;
	}

	GENERATE_BINDINGS inline float GetTickDeltaTime()
	{
		return FindInstance().m_tickDeltaTime;
	}

	GENERATE_BINDINGS inline float GetFramesPerSecond()
	{
		return 1.0f / FindInstance().m_frameDeltaTime;
	}

	GENERATE_BINDINGS inline float GetTime()
	{
		return FindInstance().m_curTime;
	}

	GENERATE_BINDINGS inline const char* GetProjectPath()
	{
		std::string str = EngineProperties::LoadedProject.GetValue();

		// Copy string so we can use it out-of-scope
		char* cstr = new char[str.length() + 1];
		strcpy_s( cstr, str.length() + 1, str.c_str() );

		return cstr;
	};

	GENERATE_BINDINGS inline bool IsServer()
	{
		return IS_SERVER;
	}

	GENERATE_BINDINGS inline bool IsClient()
	{
		return IS_CLIENT;
	}

	GENERATE_BINDINGS inline Root GetRoot()
	{
		return FindInstance();
	}

	GENERATE_BINDINGS inline uint32_t CreateBaseEntity()
	{
		// TODO: Derive root based on current context / realm
		auto* entityDictionary = FindInstance().m_entityManager;

		BaseEntity baseEntity = {};
		baseEntity.AddFlag( ENTITY_MANAGED );
		baseEntity.m_type = "BaseEntity";

		return entityDictionary->AddEntity<BaseEntity>( baseEntity );
	}

	GENERATE_BINDINGS inline uint32_t CreateModelEntity()
	{
		auto* entityDictionary = FindInstance().m_entityManager;

		ModelEntity modelEntity = {};
		modelEntity.AddFlag( ENTITY_MANAGED );
		modelEntity.AddFlag( ENTITY_RENDERABLE );
		modelEntity.m_type = "ModelEntity";

		return entityDictionary->AddEntity<ModelEntity>( modelEntity );
	}

	GENERATE_BINDINGS inline void SetCameraPosition( Vector3 position )
	{
		FindInstance().m_cameraPos = position;
	}

	GENERATE_BINDINGS inline Vector3 GetCameraPosition()
	{
		return FindInstance().m_cameraPos;
	}

	GENERATE_BINDINGS inline void SetCameraRotation( Quaternion rotation )
	{
		FindInstance().m_cameraRot = rotation;
	}

	GENERATE_BINDINGS inline Quaternion GetCameraRotation()
	{
		return FindInstance().m_cameraRot;
	}

	GENERATE_BINDINGS inline void SetCameraFieldOfView( float fov )
	{
		FindInstance().m_cameraFov = fov;
	}

	GENERATE_BINDINGS inline float GetCameraFieldOfView()
	{
		return FindInstance().m_cameraFov;
	}

	GENERATE_BINDINGS inline void SetCameraZNear( float znear )
	{
		FindInstance().m_cameraZNear = znear;
	}

	GENERATE_BINDINGS inline float GetCameraZNear()
	{
		return FindInstance().m_cameraZNear;
	}

	GENERATE_BINDINGS inline void SetCameraZFar( float zfar )
	{
		FindInstance().m_cameraZFar = zfar;
	}

	GENERATE_BINDINGS inline float GetCameraZFar()
	{
		return FindInstance().m_cameraZFar;
	}

}; // namespace Engine
