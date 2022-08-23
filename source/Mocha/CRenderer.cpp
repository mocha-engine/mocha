#include "CRenderer.h"

#include "Assert.h"
#include "CWindow.h"

#include <format>
#include <fstream>
#include <iostream>
#include <spdlog/spdlog.h>
#include <stdio.h>

CRenderer::CRenderer( CWindow* window )
{
	mWindow = window;

	InitAPI();
	InitSwapchain();
}

CRenderer::~CRenderer()
{
	Cleanup();
}

void CRenderer::Resize( Uint2 size ) {}

void CRenderer::InitAPI()
{
	vkb::InstanceBuilder builder;
	auto inst_ret = builder.set_app_name( "Mocha Engine Game" )
	                    .request_validation_layers()
	                    .require_api_version( 1, 1, 0 )
	                    .use_default_debug_messenger()
	                    .build();

	if ( !inst_ret )
	{
		spdlog::error( "Failed to create Vulkan instance" );
	}

	vkb::Instance vkb_inst = inst_ret.value();

	mInstance = vkb_inst.instance;
	mDebugMessenger = vkb_inst.debug_messenger;

	//
	// Create a window surface using SDL
	//
	auto sdlWindow = mWindow->GetWindowPointer();
	if ( !SDL_Vulkan_CreateSurface( sdlWindow, mInstance, &mSurface ) )
	{
		spdlog::error( "Failed to create Vulkan surface" );
	}

	//
	// Select a GPU with min vk version 1.1 that can write to the SDL surface
	//
	vkb::PhysicalDeviceSelector selector{ vkb_inst };
	vkb::PhysicalDevice physicalDevice = selector.set_minimum_version( 1, 1 ).set_surface( mSurface ).select().value();

	if ( !physicalDevice )
	{
		spdlog::error( "Failed to select a physical device" );
	}

	//
	// Create a logical device
	//
	vkb::DeviceBuilder deviceBuilder{ physicalDevice };
	vkb::Device vkbDevice = deviceBuilder.build().value();

	if ( !vkbDevice )
	{
		spdlog::error( "Failed to create logical device" );
	}

	mDevice = vkbDevice.device;
	mPhysicalDevice = physicalDevice.physical_device;
}

void CRenderer::InitSwapchain()
{
	//
	// Create a swapchain
	//
	vkb::SwapchainBuilder swapchainBuilder{ mPhysicalDevice, mDevice, mSurface };
	vkb::Swapchain swapchain = swapchainBuilder.use_default_format_selection()
	                               .set_desired_present_mode( VK_PRESENT_MODE_FIFO_KHR )
	                               .set_desired_extent( 1280, 720 )
	                               .build()
	                               .value();

	mSwapchain = swapchain.swapchain;
	mSwapchainImageFormat = swapchain.image_format;
	mSwapchainImages = swapchain.get_images().value();
	mSwapchainImageViews = swapchain.get_image_views().value();

	if ( mSwapchainImages.size() == 0 )
	{
		spdlog::error( "Failed to create swapchain" );
	}
}

void CRenderer::Cleanup()
{
	vkDestroySwapchainKHR( mDevice, mSwapchain, nullptr );

	for ( size_t i = 0; i < mSwapchainImageViews.size(); i++ )
	{
		vkDestroyImageView( mDevice, mSwapchainImageViews[i], nullptr );
	}

	vkDestroyDevice( mDevice, nullptr );
	vkDestroySurfaceKHR( mInstance, mSurface, nullptr );

	vkb::destroy_debug_utils_messenger( mInstance, mDebugMessenger );

	vkDestroyInstance( mInstance, nullptr );
}

void CRenderer::BeginFrame() {}

void CRenderer::EndFrame() {}