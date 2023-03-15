using ClangSharp.Interop;

namespace MochaTool.InteropGen;

/// <summary>
/// Contains extension methods for the <see cref="CXCursor"/>.
/// </summary>
internal static class CXCursorExtensions
{
	/// <summary>
	/// Returns whether or not the current item the cursor is over has the "generate_bindings" attribute on it.
	/// </summary>
	/// <param name="cursor">The cursor to check.</param>
	/// <returns>Whether or not the current item the cursor is over has the "generate_bindings" attribute on it.</returns>
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
