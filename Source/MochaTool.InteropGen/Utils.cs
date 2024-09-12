using MochaTool.InteropGen.Extensions;
using System.CodeDom.Compiler;
using System.Collections.Frozen;

namespace MochaTool.InteropGen;

/// <summary>
/// Contains a number of utility methods.
/// </summary>
internal static class Utils
{
	/// <summary>
	/// Used as a lookup table for mapping native types to managed ones.
	/// </summary>
	private static readonly FrozenDictionary<string, string> s_lookupTable = new Dictionary<string, string>()
	{
		// Native type		Managed type
		//-------------------------------
		{ "void", "void" },
		{ "uint32_t", "uint" },
		{ "int32_t", "int" },
		{ "size_t", "uint" },

		{ "char**", "ref string" },
		{ "char **", "ref string" },
		{ "char*", "string" },
		{ "char *", "string" },
		{ "void*", "IntPtr" },
		{ "void *", "IntPtr" },

		// STL
		{ "std::string", "/* UNSUPPORTED */ string" },

		// GLM
		{ "glm::vec2", "Vector2" },
		{ "glm::vec3", "Vector3" },
		{ "glm::mat4", "Matrix4x4" },
		{ "glm::quat", "Rotation" },

		// Custom
		{ "Quaternion", "Rotation" },
		{ "InteropStruct", "IInteropArray" },
		{ "Handle", "uint" }
	}.ToFrozenDictionary();

	/// <summary>
	/// Returns whether or not the string represents a pointer.
	/// </summary>
	/// <param name="nativeType">The native type to check.</param>
	/// <returns>Whether or not the string represents a pointer.</returns>
	internal static bool IsPointer( string nativeType )
	{
		var managedType = GetManagedType( nativeType );
		return nativeType.Trim().EndsWith( "*" ) && managedType != "string" && managedType != "IntPtr";
	}

	/// <summary>
	/// Returns the C# version of a native type.
	/// </summary>
	/// <param name="nativeType">The native type to check.</param>
	/// <returns>The C# verison of a native type.</returns>
	internal static string GetManagedType( string nativeType )
	{
		// Trim whitespace from beginning / end (if it exists)
		nativeType = nativeType.Trim();

		// Remove the "const" keyword
		if ( nativeType.StartsWith( "const" ) )
			nativeType = nativeType[5..].Trim();

		// Check if the native type is a reference
		if ( nativeType.EndsWith( "&" ) )
			return GetManagedType( nativeType[0..^1] );

		// Check if the native type is in the lookup table
		if ( s_lookupTable.TryGetValue( nativeType, out var value ) )
		{
			// Bonus: Emit a compiler warning if the native type is std::string
			if ( nativeType == "std::string" )
			{
				// There's a better API that does this but I can't remember what it is
				// TODO: Show position of the warning (line number, file name)
				Log.WarnDiagnostic( "warning IG0001: std::string is not supported in managed code. Use a C string instead." );
			}

			return value;
		}

		// Check if the native type is a pointer
		if ( nativeType.EndsWith( "*" ) )
			return GetManagedType( nativeType[..^1].Trim() ); // We'll return the basic type, because we handle pointers on the C# side now

		// Return the native type if it is not in the lookup table
		return nativeType;
	}

	/// <summary>
	/// Creates and returns the text writer for writing formatted files.
	/// </summary>
	/// <returns>The created text writer.</returns>
	internal static (StringWriter StringWriter, IndentedTextWriter TextWriter) CreateWriter()
	{
		var baseTextWriter = new StringWriter();

		var writer = new IndentedTextWriter( baseTextWriter, "    " )
		{
			Indent = 0
		};

		return (baseTextWriter, writer);
	}
}
