#pragma once
#include <Misc/defs.h>
#include <Misc/mathtypes.h>
#include <Root/clientroot.h>
#include <stdint.h>
#include <string>

class Camera;
class Model;

enum SceneMeshFlags
{
	SCENE_MESH_FLAGS_WORLD_LAYER = 1 << 1,
	SCENE_MESH_FLAGS_UI_LAYER = 1 << 2,

	SCENE_MESH_FLAGS_DEFAULT = SCENE_MESH_FLAGS_WORLD_LAYER,
	SCENE_MESH_FLAGS_POSTPROCESS = SCENE_MESH_FLAGS_UI_LAYER,
};

class SceneMesh
{
private:
	Model* m_model;
	SceneMeshFlags m_flags = SCENE_MESH_FLAGS_DEFAULT;

public:
	SceneMesh()
	    : m_spawnTime( Globals::m_curTick ){};

	virtual ~SceneMesh() {}

	int m_spawnTime;

	Transform m_transformLastFrame = {};
	Transform m_transformCurrentFrame = {};

	Transform m_transform = {};

	//
	// Managed bindings
	//
	GENERATE_BINDINGS inline void SetTransform( const Transform& tx ) { m_transform = tx; }
	GENERATE_BINDINGS inline Transform GetTransform() { return m_transform; }

	GENERATE_BINDINGS void SetModel( Model* model ) { m_model = model; }
	GENERATE_BINDINGS Model* GetModel() { return m_model; }

	GENERATE_BINDINGS void SetFlags( SceneMeshFlags flags ) { m_flags = flags; }
	GENERATE_BINDINGS SceneMeshFlags GetFlags() { return m_flags; }
};
