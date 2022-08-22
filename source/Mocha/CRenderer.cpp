#include "CRenderer.h"

#include "Assert.h"
#include "CWindow.h"

#include <format>
#include <fstream>
#include <iostream>
#include <spdlog/spdlog.h>

CRenderer::CRenderer( CWindow* window ) {}

CRenderer::~CRenderer() {}

void CRenderer::InitAPI( CWindow* window ) {}

void CRenderer::InitShaderResources() {}

void CRenderer::DestroyShaderResources() {}

void CRenderer::InitFramebuffer() {}

void CRenderer::DestroyFramebuffer() {}

void CRenderer::InitResources() {}

void CRenderer::DestroyResources() {}

void CRenderer::CreateCommands() {}

void CRenderer::DestroyCommands() {}

void CRenderer::SetupSwapchain( unsigned width, unsigned height ) {}

void CRenderer::Resize( Uint2 size ) {}

void CRenderer::BeginFrame() {}

void CRenderer::EndFrame() {}