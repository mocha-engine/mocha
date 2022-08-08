#include "CEditor.h"

CEditor::CEditor()
{
	char_t hostPath[MAX_PATH];
	auto size = GetCurrentDirectory( MAX_PATH, hostPath );
	assert( size != 0 );

	string_t rootPath = hostPath;
	string_t engine_dir = rootPath + STR( "\\" );

	// clang-format off
	CNetCoreHost netCoreHost( engine_dir + STR( "Editor.runtimeconfig.json" ), engine_dir + STR( "Editor.dll" ) );
	
	editor_entry_fn mainFunction = (editor_entry_fn)netCoreHost.FindFunction(
		STR("Mocha.Editor.Program, Editor"),
		STR("Main")
	);
	
	mManagedRenderFunction = ( editor_render_fn )netCoreHost.FindFunction( 
		STR( "Mocha.Editor.Program, Editor" ), 
		STR( "Render" ) 
	);
	
	mainFunction( &args );
	// clang-format on
}

void CEditor::Render()
{
	mManagedRenderFunction();
}
