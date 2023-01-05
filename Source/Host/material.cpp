#include "material.h"

#include <globalvars.h>
#include <model.h>
#include <rendering.h>
#include <rendermanager.h>
#include <vkinit.h>

Material::Material(
    const char* shaderPath, UtilArray vertexAttributes, UtilArray textures, SamplerType samplerType, bool ignoreDepth )
{
	auto texturePtrs = textures.GetData<Texture*>();
	m_textures = std::vector<Texture>( textures.count );

	for ( int i = 0; i < textures.count; i++ )
	{
		m_textures[i] = Texture( *texturePtrs[i] );
	}

	m_shaderPath = std::string( shaderPath );

	auto vertexAttribInfo = vertexAttributes.GetData<InteropVertexAttributeInfo>();
	for ( int i = 0; i < vertexAttributes.count; i++ )
	{
		m_vertexAttribInfo.push_back( vertexAttribInfo[i].ToNative() );
	}

	m_samplerType = samplerType;
	m_ignoreDepth = ignoreDepth;
}

void Material::CreateResources()
{
	PipelineInfo_t pipelineInfo = {};

	pipelineInfo.shaderInfo = {};
	pipelineInfo.shaderInfo.shaderPath = m_shaderPath;
	pipelineInfo.vertexAttributes = m_vertexAttribInfo;

	DescriptorInfo_t descriptorInfo;
	descriptorInfo.bindings = {};

	for ( int i = 0; i < m_textures.size(); ++i )
	{
		Texture texture = m_textures[i];

		DescriptorBindingInfo_t bindingInfo = {};
		bindingInfo.type = DESCRIPTOR_BINDING_TYPE_IMAGE;
		bindingInfo.texture = &texture.m_image;

		descriptorInfo.bindings.push_back( bindingInfo );
	}

	m_descriptor = Descriptor( descriptorInfo );
	pipelineInfo.descriptors.push_back( &m_descriptor );

	m_pipeline = Pipeline( pipelineInfo );
}

void Material::ReloadShaders()
{
	CreateResources();
}
