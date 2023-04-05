using System.Collections.Immutable;

namespace MochaTool.InteropGen.Parsing;

/// <summary>
/// Defines a container for fields and methods defined in C++.
/// </summary>
internal interface IUnit
{
	/// <summary>
	/// The name of the <see cref="IUnit"/>.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// All of the fields contained in the <see cref="IUnit"/>.
	/// </summary>
	ImmutableArray<Variable> Fields { get; }
	/// <summary>
	/// All of the methods contained in the <see cref="IUnit"/>.
	/// </summary>
	ImmutableArray<Method> Methods { get; }

	/// <summary>
	/// Returns a new instance of the <see cref="IUnit"/> with the fields given.
	/// </summary>
	/// <param name="fields">The new fields to place in the instance.</param>
	/// <returns>A new instance of the <see cref="IUnit"/> with the fields given.</returns>
	IUnit WithFields( in ImmutableArray<Variable> fields );
	/// <summary>
	/// Returns a new instance of the <see cref="IUnit"/> with the methods given.
	/// </summary>
	/// <param name="methods">The new methods to place in the instance.</param>
	/// <returns>A new instance of the <see cref="IUnit"/> with the methods given.</returns>
	IUnit WithMethods( in ImmutableArray<Method> methods );
}
