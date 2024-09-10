#pragma once
#include <Misc/defs.h>
#include <Misc/mathtypes.h>
#include <Root/clientroot.h>
#include <stdint.h>
#include <string>

class Camera;
class Model;

class SceneMesh
{
private:
	Model* m_model;

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
};
