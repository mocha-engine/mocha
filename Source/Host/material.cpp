#include "material.h"

#include <filesystemwatcher.h>
#include <globalvars.h>
#include <model.h>
#include <rendering.h>
#include <rendermanager.h>
#include <vkinit.h>

Material::Material( UtilArray vertexShaderData, UtilArray fragmentShaderData, UtilArray vertexAttributes, UtilArray textures,
    SamplerType samplerType, bool ignoreDepth )
{
	m_vertexShaderData = vertexShaderData.GetData<uint32_t>();	
	m_fragmentShaderData = fragmentShaderData.GetData<uint32_t>();
	
	m_isDirty.store( true );

	auto texturePtrs = textures.GetData<Texture*>();
	m_textures = std::vector<Texture>( textures.count );

	for ( int i = 0; i < textures.count; i++ )
	{
		m_textures[i] = Texture( *texturePtrs[i] );
	}

	auto vertexAttribInfo = vertexAttributes.GetData<InteropVertexAttributeInfo>();
	for ( int i = 0; i < vertexAttributes.count; i++ )
	{
		m_vertexAttribInfo.push_back( vertexAttribInfo[i].ToNative() );
	}

	m_samplerType = samplerType;
	m_ignoreDepth = ignoreDepth;
}

void Material::Reload()
{
	m_isDirty.store( true );
}

void Material::CreateResources()
{
	spdlog::trace( "Material::CreateResources()" );
	PipelineInfo_t pipelineInfo = {};

	pipelineInfo.shaderInfo = {};
	pipelineInfo.shaderInfo.vertexShaderData = m_vertexShaderData;
	pipelineInfo.shaderInfo.fragmentShaderData = m_fragmentShaderData;
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

	m_isDirty.store( false );
}
