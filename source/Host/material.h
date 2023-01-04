#pragma once
#include <defs.h>
#include <rendering.h>
#include <texture.h>
#include <vector>

struct InteropVertexAttributeInfo
{
	const char* name;
	VertexAttributeFormat format;

	VertexAttributeInfo_t ToNative()
	{
		VertexAttributeInfo_t native = {};
		native.name = name;
		native.format = format;

		return native;
	}
};

//@InteropGen generate class
class Material
{
public:
	std::vector<Texture> m_textures;
	std::string m_shaderPath;

	void CreateResources();
	void ReloadShaders();

	SamplerType m_samplerType = {};
	Descriptor m_descriptor = {};
	Pipeline m_pipeline = {};

	std::vector<VertexAttributeInfo_t> m_vertexAttribInfo;

	bool m_ignoreDepth;

	Material(
	    const char* shaderPath, UtilArray vertexAttributes, UtilArray textures, SamplerType samplerType, bool ignoreDepth );
};