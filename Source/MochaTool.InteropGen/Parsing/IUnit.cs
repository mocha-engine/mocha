namespace MochaTool.InteropGen.Parsing;

/// <summary>
/// Defines any declaration in C++.
/// </summary>
internal interface IUnit
{
	/// <summary>
	/// The name of the <see cref="IUnit"/>.
	/// </summary>
	string Name { get; }
}
