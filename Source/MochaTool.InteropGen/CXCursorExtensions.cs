using ClangSharp.Interop;

namespace MochaTool.InteropGen;

internal static class CXCursorExtensions
{
	internal static bool HasGenerateBindingsAttribute( this CXCursor cursor )
	{
		if ( !cursor.HasAttrs )
			return false;

		var attr = cursor.GetAttr( 0 );
		if ( attr.Spelling.CString != "generate_bindings" )
			return false;

		return true;
	}
}
