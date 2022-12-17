#pragma once

#include "game/model.h"
#include "vulkan/mesh.h"
#include <managedtypes.h>

class ManagedMaterial;

//@InteropGen generate class
class ManagedModel
{
private:
	Model m_model;

public:
	// Add a mesh through a vertex buffer and (optionally) a vertex buffer.
	// The texture specified will be used as the diffuse texture for this mesh.
	void AddMesh( InteropStruct vertices, InteropStruct indices, ManagedMaterial* material );

	//@InteropGen ignore
	Model GetModel();
};