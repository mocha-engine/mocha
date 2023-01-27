cmake_minimum_required(VERSION 3.21)

project(Mocha)

# https://stackoverflow.com/a/2561377/8176082
add_compile_definitions(NOMINMAX)

set(VCPKG_TARGET_TRIPLET "x64-windows" CACHE STRING "")
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_GENERATOR_PLATFORM x64)
add_compile_definitions(_DEBUG;_IMGUI)

find_package(Vulkan)

set(VULKAN_PATH ${Vulkan_INCLUDE_DIRS})
STRING(REGEX REPLACE "/Include" "" VULKAN_PATH ${VULKAN_PATH})
message(STATUS "Vulkan path: ${VULKAN_PATH}")

find_package(unofficial-nethost CONFIG REQUIRED)
find_package(glslang CONFIG REQUIRED)
find_package(spdlog CONFIG REQUIRED)
find_package(sdl2 CONFIG REQUIRED)
find_package(glm CONFIG REQUIRED)
find_package(unofficial-vulkan-memory-allocator CONFIG REQUIRED)

include_directories(${VULKAN_PATH}/Include ${GLM_INCLUDE_DIRS} ${VULKAN_MEMORY_ALLOCATOR_INCLUDE_DIRS} ${SDL_INCLUDE_DIRS})
link_directories( ${VULKAN_PATH}/Bin;${VULKAN_PATH}/Lib; )

file(GLOB HOST_SOURCES "source/Host/*.cpp")
file(GLOB HOST_HEADERS "source/Host/*.h")
file(GLOB_RECURSE JOLT_SOURCES "source/Host/thirdparty/JoltPhysics/Jolt/*.cpp")
file(GLOB_RECURSE VKB_SOURCES "source/Host/thirdparty/vk-bootstrap/src/*.cpp")

set(IMGUI_SOURCES
	"source/Host/thirdparty/imgui/imgui.cpp"
	"source/Host/thirdparty/imgui/imgui_demo.cpp"
	"source/Host/thirdparty/imgui/imgui_draw.cpp"
	"source/Host/thirdparty/imgui/imgui_tables.cpp"
	"source/Host/thirdparty/imgui/imgui_widgets.cpp"
	"source/Host/thirdparty/imgui/backends/imgui_impl_sdl.cpp"
	"source/Host/thirdparty/imgui/backends/imgui_impl_vulkan.cpp"
)

include_directories("source/Host/")
include_directories("source/Host/thirdparty/JoltPhysics")
include_directories("source/Host/thirdparty/imgui")
include_directories("source/Host/thirdparty/renderdoc")
include_directories("source/Host/thirdparty/vk-bootstrap/src")

add_executable(Host 
	${HOST_SOURCES}
	${HOST_HEADERS}
	
	${JOLT_SOURCES}
	${IMGUI_SOURCES}
	${VKB_SOURCES}
)

target_link_libraries(Host PRIVATE
	vulkan-1
	unofficial::vulkan-memory-allocator::vulkan-memory-allocator
	glm::glm
	SDL2::SDL2 SDL2::SDL2main
	spdlog::spdlog spdlog::spdlog_header_only
	HLSL SPIRV glslang OGLCompiler
	unofficial::nethost::nethost
)

set_property(TARGET Host PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}")