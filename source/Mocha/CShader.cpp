#include "CShader.h"

#include "Assert.h"
#include "CMochaEngine.h"
#include "CRenderer.h"
#include "Globals.h"

#include <spdlog/spdlog.h>

CShader::CShader( const char* path, const char* source )
{
	mPath = path;
	mSource = source;
}

CShader::~CShader() {}

bool CShader::Compile()
{
	return true;
}
