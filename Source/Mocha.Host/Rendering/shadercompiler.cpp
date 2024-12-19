#include "shadercompiler.h"

#include "baserendercontext.h"
#include <vector>
#include "spdlog/spdlog.h"

using namespace slang;

ShaderCompiler::ShaderCompiler()
{
	createGlobalSession( m_globalSession.writeRef() );
}

ShaderCompiler::~ShaderCompiler() { }

bool ShaderCompiler::Compile( const ShaderType shaderType, const char* pShader, std::vector<uint32_t>& outSpirv )
{
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
	IModule* module =
	    session->loadModuleFromSourceString( "Shader", "Shader.slang", pShader, diagnostics.writeRef() );

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
	outSpirv = std::vector<uint32_t>( data, data + wordCount );
	return true;
}