#include "material.h"

#include <Misc/globalvars.h>
#include <Rendering/Assets/model.h>
#include <Rendering/Platform/Vulkan/vkinit.h>
#include <Rendering/rendering.h>
#include <Rendering/rendermanager.h>
#include <Root/root.h>

Material::Material( const char* name, UtilArray vertexShaderData, UtilArray fragmentShaderData,
    UtilArray vertexAttributes, UtilArray textures, SamplerType samplerType, bool ignoreDepth )
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
	m_name = std::string( name );
}

Material::Material( const char* name, UtilArray vertexShaderData, UtilArray fragmentShaderData, UtilArray vertexAttributes )
{
	m_vertexShaderData = vertexShaderData.GetData<uint32_t>();
	m_fragmentShaderData = fragmentShaderData.GetData<uint32_t>();

	m_isDirty.store( true );

	auto vertexAttribInfo = vertexAttributes.GetData<InteropVertexAttributeInfo>();
	for ( int i = 0; i < vertexAttributes.count; i++ )
	{
		m_vertexAttribInfo.push_back( vertexAttribInfo[i].ToNative() );
	}

	m_samplerType = SAMPLER_TYPE_LINEAR;
	m_ignoreDepth = true;
	m_name = std::string( name );
}

void Material::Reload()
{
	m_isDirty.store( true );
}

void Material::SetShaderData( UtilArray vertexShaderData, UtilArray fragmentShaderData )
{
	m_vertexShaderData = vertexShaderData.GetData<uint32_t>();
	m_fragmentShaderData = fragmentShaderData.GetData<uint32_t>();
}

void Material::CreateResources()
{
	PipelineInfo_t pipelineInfo = {};

	pipelineInfo.name = m_name + " pipeline";
	pipelineInfo.shaderInfo = {};
	pipelineInfo.shaderInfo.vertexShaderData = m_vertexShaderData;
	pipelineInfo.shaderInfo.fragmentShaderData = m_fragmentShaderData;
	pipelineInfo.vertexAttributes = m_vertexAttribInfo;
	pipelineInfo.ignoreDepth = m_ignoreDepth;

	DescriptorInfo_t descriptorInfo;
	descriptorInfo.name = m_name + " descriptor";
	descriptorInfo.bindings = {};

	for ( int i = 0; i < m_textures.size(); ++i )
	{
		Texture texture = m_textures[i];

		DescriptorBindingInfo_t bindingInfo = {};
		bindingInfo.type = DESCRIPTOR_BINDING_TYPE_IMAGE;
		bindingInfo.texture = &texture.m_image;

		descriptorInfo.bindings.push_back( bindingInfo );
	}

	DescriptorBindingInfo_t samplerBindingInfo = {};
	samplerBindingInfo.type = DESCRIPTOR_BINDING_TYPE_SAMPLER;
	samplerBindingInfo.sampler = m_samplerType;
	descriptorInfo.bindings.push_back( samplerBindingInfo );

	m_descriptor = Descriptor( descriptorInfo );
	pipelineInfo.descriptors.push_back( &m_descriptor );

	m_pipeline = Pipeline( pipelineInfo );

	m_isDirty.store( false );
}
