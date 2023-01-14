#pragma once
#include <defs.h>
#include <rendering.h>
#include <texture.h>
#include <vector>

class FileSystemWatcher;

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

class Material
{
private:
	std::atomic<bool> m_isDirty;
	std::vector<uint32_t> m_vertexShaderData;
	std::vector<uint32_t> m_fragmentShaderData;

public:
	std::vector<Texture> m_textures;
	std::string m_shaderPath;

	GENERATE_BINDINGS void CreateResources();
	GENERATE_BINDINGS void Reload();

	SamplerType m_samplerType = {};
	Descriptor m_descriptor = {};
	Pipeline m_pipeline = {};
	std::vector<VertexAttributeInfo_t> m_vertexAttribInfo;

	bool m_ignoreDepth;
	bool IsDirty() { return m_isDirty.load( std::memory_order_relaxed ); }

	GENERATE_BINDINGS Material( UtilArray vertexShaderData, UtilArray fragmentShaderData, UtilArray vertexAttributes,
	    UtilArray textures, SamplerType samplerType, bool ignoreDepth );

	Material( const Material& other ) noexcept
	    : m_isDirty( other.m_isDirty.load() )
	    , m_textures( other.m_textures )
	    , m_shaderPath( other.m_shaderPath )
	    , m_samplerType( other.m_samplerType )
	    , m_descriptor( other.m_descriptor )
	    , m_pipeline( other.m_pipeline )
	    , m_vertexAttribInfo( other.m_vertexAttribInfo )
	    , m_ignoreDepth( other.m_ignoreDepth )
	{
	}

	Material& operator=( const Material& other ) noexcept
	{
		if ( this == &other )
			return *this;
		m_isDirty.store( other.m_isDirty.load() );
		m_textures = other.m_textures;
		m_shaderPath = other.m_shaderPath;
		m_samplerType = other.m_samplerType;
		m_descriptor = other.m_descriptor;
		m_pipeline = other.m_pipeline;
		m_vertexAttribInfo = other.m_vertexAttribInfo;
		m_ignoreDepth = other.m_ignoreDepth;
		return *this;
	}
};