#include "shadercompiler.h"

#include "baserendercontext.h"
#include "spdlog/spdlog.h"

#include <vector>

using namespace slang;

ShaderCompiler::ShaderCompiler()
{
	createGlobalSession( m_globalSession.writeRef() );
}

ShaderCompiler::~ShaderCompiler()
{
}

const char* KindToString( slang::TypeReflection::Kind kind )
{
	switch ( kind )
	{
	case slang::TypeReflection::Kind::None:
		return "None";
	case slang::TypeReflection::Kind::Struct:
		return "Struct";
	case slang::TypeReflection::Kind::Array:
		return "Array";
	case slang::TypeReflection::Kind::Matrix:
		return "Matrix";
	case slang::TypeReflection::Kind::Vector:
		return "Vector";
	case slang::TypeReflection::Kind::Scalar:
		return "Scalar";
	case slang::TypeReflection::Kind::ConstantBuffer:
		return "ConstantBuffer";
	case slang::TypeReflection::Kind::Resource:
		return "Resource";
	case slang::TypeReflection::Kind::SamplerState:
		return "SamplerState";
	case slang::TypeReflection::Kind::TextureBuffer:
		return "TextureBuffer";
	case slang::TypeReflection::Kind::ShaderStorageBuffer:
		return "ShaderStorageBuffer";
	case slang::TypeReflection::Kind::ParameterBlock:
		return "ParameterBlock";
	case slang::TypeReflection::Kind::GenericTypeParameter:
		return "GenericTypeParameter";
	case slang::TypeReflection::Kind::Interface:
		return "Interface";
	case slang::TypeReflection::Kind::OutputStream:
		return "OutputStream";
	case slang::TypeReflection::Kind::Specialized:
		return "Specialized";
	case slang::TypeReflection::Kind::Feedback:
		return "Feedback";
	case slang::TypeReflection::Kind::Pointer:
		return "Pointer";
	case slang::TypeReflection::Kind::DynamicResource:
		return "DynamicResource";
	default:
		return "Unknown";
	}
}

const char* BindingTypeToString( slang::BindingType type )
{
	switch ( type )
	{
	case slang::BindingType::Unknown:
		return "Unknown";
	case slang::BindingType::Sampler:
		return "Sampler";
	case slang::BindingType::Texture:
		return "Texture";
	case slang::BindingType::ConstantBuffer:
		return "ConstantBuffer";
	case slang::BindingType::ParameterBlock:
		return "ParameterBlock";
	case slang::BindingType::TypedBuffer:
		return "TypedBuffer";
	case slang::BindingType::RawBuffer:
		return "RawBuffer";
	case slang::BindingType::CombinedTextureSampler:
		return "CombinedTextureSampler";
	case slang::BindingType::InputRenderTarget:
		return "InputRenderTarget";
	case slang::BindingType::InlineUniformData:
		return "InlineUniformData";
	case slang::BindingType::RayTracingAccelerationStructure:
		return "RayTracingAccelerationStructure";
	case slang::BindingType::VaryingInput:
		return "VaryingInput";
	case slang::BindingType::VaryingOutput:
		return "VaryingOutput";
	case slang::BindingType::ExistentialValue:
		return "ExistentialValue";
	case slang::BindingType::PushConstant:
		return "PushConstant";
	case slang::BindingType::MutableFlag:
		return "MutableFlag";
	case slang::BindingType::MutableTexture:
		return "MutableTexture";
	case slang::BindingType::MutableTypedBuffer:
		return "MutableTypedBuffer";
	case slang::BindingType::MutableRawBuffer:
		return "MutableRawBuffer";
	case slang::BindingType::BaseMask:
		return "BaseMask";
	case slang::BindingType::ExtMask:
		return "ExtMask";
	default:
		return "Unknown";
	}
}

bool ShaderCompiler::Compile( const ShaderType shaderType, const char* pShader, ShaderCompilerResult& outResult )
{
	outResult = ShaderCompilerResult();

	Slang::ComPtr<ISession> session;

	TargetDesc targetDesc{};
	targetDesc.format = SLANG_SPIRV;

	if ( shaderType == SHADER_TYPE_FRAGMENT )
		targetDesc.profile = m_globalSession->findProfile( "spirv_1_3" );
	else if ( shaderType == SHADER_TYPE_VERTEX )
		targetDesc.profile = m_globalSession->findProfile( "spirv_1_3" );

	targetDesc.flags = SLANG_TARGET_FLAG_GENERATE_SPIRV_DIRECTLY | SLANG_TARGET_FLAG_GENERATE_WHOLE_PROGRAM;

	SessionDesc sessionDesc{};
	sessionDesc.targets = &targetDesc;
	sessionDesc.targetCount = 1;

	m_globalSession->createSession( sessionDesc, session.writeRef() );

	Slang::ComPtr<IBlob> diagnostics;
	IModule* module = session->loadModuleFromSourceString( "Shader", "Shader.slang", pShader, diagnostics.writeRef() );

	if ( diagnostics )
		spdlog::error( "Shader compiler: {}", ( const char* )diagnostics->getBufferPointer() );

	spdlog::info( "Entry point count: {}", module->getDefinedEntryPointCount() );

	Slang::ComPtr<IEntryPoint> entryPoint;

	module->findEntryPointByName( "main", entryPoint.writeRef() );

	IComponentType* components[] = { module, entryPoint };
	Slang::ComPtr<IComponentType> program;
	session->createCompositeComponentType( components, 2, program.writeRef(), diagnostics.writeRef() );

	if ( diagnostics )
		spdlog::error( "Shader compiler: {}", ( const char* )diagnostics->getBufferPointer() );

	Slang::ComPtr<IComponentType> linkedProgram;
	Slang::ComPtr<ISlangBlob> diagnosticBlob;
	program->link( linkedProgram.writeRef(), diagnosticBlob.writeRef() );

	int entryPointIndex = 0;
	int targetIndex = 0;

	Slang::ComPtr<IBlob> kernelBlob;
	linkedProgram->getEntryPointCode( entryPointIndex, targetIndex, kernelBlob.writeRef(), diagnostics.writeRef() );

	if ( diagnostics )
		spdlog::error( "Shader compiler: {}", ( const char* )diagnostics->getBufferPointer() );

	const uint32_t* data = static_cast<const uint32_t*>( kernelBlob->getBufferPointer() );
	size_t wordCount = kernelBlob->getBufferSize() / sizeof( uint32_t );

	outResult.ShaderData = UtilArray::FromVector( std::vector<uint32_t>( data, data + wordCount ) );

	std::vector<ShaderReflectionBinding> reflectionBindings = {};
	ShaderReflectionInfo shaderReflectionInfo = {};
	
	{
		slang::ProgramLayout* layout = program->getLayout( targetIndex );
		auto globalScope = layout->getGlobalParamsVarLayout();
		auto globalTypeLayout = globalScope->getTypeLayout();

		int paramCount = globalTypeLayout->getFieldCount();
		for ( int i = 0; i < paramCount; i++ )
		{
			auto param = globalTypeLayout->getFieldByIndex( i );
			auto type = globalTypeLayout->getBindingRangeType( i );

			// get binding info
			size_t binding = param->getOffset( SLANG_PARAMETER_CATEGORY_DESCRIPTOR_TABLE_SLOT );
			size_t set = param->getBindingSpace( SLANG_PARAMETER_CATEGORY_DESCRIPTOR_TABLE_SLOT );

			SlangResourceShape shape = param->getType()->getResourceShape();

			// get param name/type
			const char* name = param->getName();
			slang::TypeReflection::Kind kind = param->getType()->getKind();

			spdlog::info( "[{}, {}] {} {}, {}", binding, set, KindToString( kind ), name, BindingTypeToString( type ) );

			auto mochaReflectionType = SHADER_REFLECTION_TYPE_UNKNOWN;
			switch ( type )
			{
			case slang::BindingType::Unknown:
				mochaReflectionType = SHADER_REFLECTION_TYPE_UNKNOWN;
				break;
			case slang::BindingType::Texture:
				mochaReflectionType = SHADER_REFLECTION_TYPE_TEXTURE;
				break;
			case slang::BindingType::Sampler:
				mochaReflectionType = SHADER_REFLECTION_TYPE_SAMPLER;
				break;
			case slang::BindingType::ConstantBuffer:
				mochaReflectionType = SHADER_REFLECTION_TYPE_BUFFER;
				break;
			}

			reflectionBindings.push_back( ShaderReflectionBinding{
			    .Set = ( int )set, .Binding = ( int )binding, .Type = mochaReflectionType, .Name = name } );
		}
	}

	outResult.ReflectionData = ShaderReflectionInfo { .Bindings = UtilArray::FromVector( reflectionBindings ) };

	return true;
}