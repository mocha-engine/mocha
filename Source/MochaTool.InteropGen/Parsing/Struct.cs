using System.Collections.Immutable;

namespace MochaTool.InteropGen.Parsing;

/// <summary>
/// Represents a struct in C++.
/// </summary>
internal sealed class Struct : IUnit
{
	/// <inheritdoc/>
	public string Name { get; }

	/// <inheritdoc/>
	public ImmutableArray<Variable> Fields { get; }
	/// <inheritdoc/>
	public ImmutableArray<Method> Methods { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="Struct"/>.
	/// </summary>
	/// <param name="name">The name of the struct.</param>
	/// <param name="fields">The fields contained in the struct.</param>
	/// <param name="methods">The methods contained in the struct.</param>
	private Struct( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		Name = name;

		Fields = fields;
		Methods = methods;
	}

	/// <summary>
	/// Returns a new instance of the <see cref="Struct"/> with the fields given.
	/// </summary>
	/// <param name="fields">The new fields to place in the instance.</param>
	/// <returns>A new instance of the <see cref="Struct"/> with the fields given.</returns>
	internal Struct WithFields( in ImmutableArray<Variable> fields )
	{
		return new( Name, fields, Methods );
	}

	/// <summary>
	/// Returns a new instance of the <see cref="Struct"/> with the methods given.
	/// </summary>
	/// <param name="methods">The new methods to place in the instance.</param>
	/// <returns>A new instance of the <see cref="Struct"/> with the methods given.</returns>
	internal Struct WithMethods( in ImmutableArray<Method> methods )
	{
		return new( Name, Fields, methods );
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return Name;
	}

	/// <inheritdoc/>
	IUnit IUnit.WithFields( in ImmutableArray<Variable> fields ) => WithFields( fields );
	/// <inheritdoc/>
	IUnit IUnit.WithMethods( in ImmutableArray<Method> methods ) => WithMethods( methods );

	/// <summary>
	/// Returns a new instance of <see cref="Struct"/>.
	/// </summary>
	/// <param name="name">The name of the struct.</param>
	/// <param name="fields">The fields contained in the struct.</param>
	/// <param name="methods">The methods contained in the struct.</param>
	/// <returns>A new instance of <see cref="Struct"/>.</returns>
	internal static Struct NewStructure( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		return new( name, fields, methods );
	}
}
