#pragma once

class CImgui;
class CMochaEngine;
typedef struct VkDevice_T* VkDevice;

extern CMochaEngine* g_Engine;
extern CImgui* g_Imgui;
extern VkDevice* g_Device;

extern bool g_EngineIsRunning;