#pragma once
#include <Misc/defs.h>
#include <Misc/globalvars.h>
#include <Misc/subsystem.h>
#include <Rendering/Assets/texture.h>
#include <Root/root.h>
#include <Util/util.h>

class EditorManager : ISubSystem
{
public:
	void Startup() override;
	void Shutdown() override;

	/// <summary>
	/// Get the current pointer to an ImGUI context.
	/// This is used in order to effectively "link" managed ImGUI
	/// to our native ImGUI instance.
	/// </summary>
	GENERATE_BINDINGS void* GetContextPointer();
	GENERATE_BINDINGS void TextBold( const char* text );
	GENERATE_BINDINGS void TextSubheading( const char* text );
	GENERATE_BINDINGS void TextHeading( const char* text );
	GENERATE_BINDINGS void TextMonospace( const char* text );
	GENERATE_BINDINGS void TextLight( const char* text );
	GENERATE_BINDINGS const char* GetGPUName();
	GENERATE_BINDINGS char* InputText( const char* name, char* inputBuf, int inputLength );
	GENERATE_BINDINGS void RenderViewDropdown();
	GENERATE_BINDINGS void Image( Texture* texture, uint32_t textureWidth, uint32_t textureHeight, int x, int y );
	GENERATE_BINDINGS bool BeginMainStatusBar();
	GENERATE_BINDINGS void DrawGraph( const char* name, Vector4 color, UtilArray values );
};
