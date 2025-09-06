using System.Collections.Immutable;

namespace MochaTool.InteropGen.Parsing;

/// <summary>
/// Defines a container declaration in C++.
/// </summary>
internal interface IContainerUnit : IUnit
{
	/// <summary>
	/// All of the fields contained in the <see cref="IContainerUnit"/>.
	/// </summary>
	ImmutableArray<Variable> Fields { get; }
	/// <summary>
	/// All of the methods contained in the <see cref="IContainerUnit"/>.
	/// </summary>
	ImmutableArray<Method> Methods { get; }

	/// <summary>
	/// Returns a new instance of the <see cref="IContainerUnit"/> with the fields given.
	/// </summary>
	/// <param name="fields">The new fields to place in the instance.</param>
	/// <returns>A new instance of the <see cref="IContainerUnit"/> with the fields given.</returns>
	IContainerUnit WithFields( in ImmutableArray<Variable> fields );
	/// <summary>
	/// Returns a new instance of the <see cref="IContainerUnit"/> with the methods given.
	/// </summary>
	/// <param name="methods">The new methods to place in the instance.</param>
	/// <returns>A new instance of the <see cref="IContainerUnit"/> with the methods given.</returns>
	IContainerUnit WithMethods( in ImmutableArray<Method> methods );
}
