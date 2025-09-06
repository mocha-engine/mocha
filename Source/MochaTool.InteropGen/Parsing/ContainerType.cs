namespace MochaTool.InteropGen.Parsing;

/// <summary>
/// Defines a type of container defined in C++.
/// </summary>
internal enum ContainerType : byte
{
	Class,
	Namespace,
	Struct,
	Invalid = 255
}
