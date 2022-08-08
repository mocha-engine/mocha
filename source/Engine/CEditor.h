#pragma once
#include "CNetCoreHost.h"
#include "generated/UnmanagedArgs.generated.h"

class CEditor
{
private:
	typedef void( CORECLR_DELEGATE_CALLTYPE* editor_entry_fn )( UnmanagedArgs* );
	typedef void ( *editor_render_fn )( void );
	editor_render_fn mManagedRenderFunction;

public:
	CEditor();
	void Render();
};
