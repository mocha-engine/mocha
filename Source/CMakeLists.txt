cmake_minimum_required(VERSION 3.21)

project(Mocha)

# https://stackoverflow.com/a/2561377/8176082
add_compile_definitions(NOMINMAX)

set(VCPKG_TARGET_TRIPLET "x64-windows" CACHE STRING "")
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_GENERATOR_PLATFORM x64)
add_compile_definitions(_DEBUG;_IMGUI;VK_NO_PROTOTYPES)

# Threaded build
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /MP")

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

file(GLOB HOST_SOURCES "Host/*.cpp")
file(GLOB HOST_HEADERS "Host/*.h")
file(GLOB_RECURSE JOLT_SOURCES "Host/thirdparty/JoltPhysics/Jolt/*.cpp")
file(GLOB_RECURSE VKB_SOURCES "Host/thirdparty/vk-bootstrap/src/*.cpp")

set(IMGUI_SOURCES
	"Host/thirdparty/imgui/imgui.cpp"
	"Host/thirdparty/imgui/imgui_demo.cpp"
	"Host/thirdparty/imgui/imgui_draw.cpp"
	"Host/thirdparty/imgui/imgui_tables.cpp"
	"Host/thirdparty/imgui/imgui_widgets.cpp"
	"Host/thirdparty/imgui/backends/imgui_impl_sdl.cpp"
	"Host/thirdparty/imgui/backends/imgui_impl_vulkan.cpp"
)

set(VOLK_SOURCES "Host/thirdparty/volk/volk.c")
set(IMPLOT_SOURCES "Host/thirdparty/implot/implot.cpp" "Host/thirdparty/implot/implot_demo.cpp" "Host/thirdparty/implot/implot_items.cpp")

include_directories("Host/")
include_directories("Host/thirdparty/JoltPhysics")
include_directories("Host/thirdparty/imgui")
include_directories("Host/thirdparty/renderdoc")
include_directories("Host/thirdparty/vk-bootstrap/src")
include_directories("Host/thirdparty/volk")
include_directories("Host/thirdparty/implot")

add_executable(Host 
	${HOST_SOURCES}
	${HOST_HEADERS}
	
	${JOLT_SOURCES}
	${IMGUI_SOURCES}
	${VKB_SOURCES}

	${VOLK_SOURCES}
	${IMPLOT_SOURCES}
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