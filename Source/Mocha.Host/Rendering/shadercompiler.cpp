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

			// get attributes
			auto attributeCount = param->getVariable()->getUserAttributeCount();
			spdlog::info( "Variable {} has {} attributes", name, attributeCount );

			std::vector<ShaderReflectionAttribute> reflectedAttributes{};

			for ( int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex )
			{
				auto attribute = param->getVariable()->getUserAttributeByIndex( attributeIndex );
				auto attributeName = attribute->getName();

				auto attributeArgumentCount = attribute->getArgumentCount();

				ShaderReflectionAttribute reflectedAttribute;

				if ( strcmp( attributeName, "Default" ) == 0 )
				{
					// Default attribute
					reflectedAttribute.Type = SHADER_REFLECTION_ATTRIBUTE_TYPE_DEFAULT;
					reflectedAttribute.Data = new DefaultAttributeData();
					attribute->getArgumentValueFloat( 0, &( ( DefaultAttributeData* )reflectedAttribute.Data )->ValueR );
					attribute->getArgumentValueFloat( 1, &( ( DefaultAttributeData* )reflectedAttribute.Data )->ValueG );
					attribute->getArgumentValueFloat( 2, &( ( DefaultAttributeData* )reflectedAttribute.Data )->ValueB );
					attribute->getArgumentValueFloat( 3, &( ( DefaultAttributeData* )reflectedAttribute.Data )->ValueA );
				}
				else if ( strcmp( attributeName, "SrgbRead" ) == 0 )
				{
					// SRGB read attribute
					reflectedAttribute.Type = SHADER_REFLECTION_ATTRIBUTE_TYPE_SRGB_READ;
				}
				else
				{
					spdlog::warn( "Unhandled attribute '{}'", attributeName );
				}

				reflectedAttributes.emplace_back( reflectedAttribute );
			}

			reflectionBindings.push_back( ShaderReflectionBinding{ .Set = ( int )set,
			    .Binding = ( int )binding,
			    .Type = mochaReflectionType,
			    .Name = name,
			    .Attributes = UtilArray::FromVector<ShaderReflectionAttribute>( reflectedAttributes ) } );
		}
	}

	outResult.ReflectionData = ShaderReflectionInfo{ .Bindings = UtilArray::FromVector( reflectionBindings ) };

	return true;
}