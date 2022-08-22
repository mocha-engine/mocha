#include "CRenderer.h"

#include "Assert.h"
#include "CWindow.h"

#include <bgfx/bgfx.h>
#include <bgfx/platform.h>
#include <bx/bx.h>
#include <format>
#include <fstream>
#include <iostream>
#include <spdlog/spdlog.h>
#include <stdio.h>

CRenderer::CRenderer( CWindow* window )
{
	bgfx::renderFrame(); // Do not create render frame

	bgfx::Init init;
	init.platformData.nwh = window->GetWindowHandle();

	int width, height;
}

CRenderer::~CRenderer() {}

void CRenderer::Resize( Uint2 size ) {}

void CRenderer::BeginFrame() {}

void CRenderer::EndFrame() {}