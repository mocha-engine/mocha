using System.Collections.Immutable;

namespace MochaTool.InteropGen.Parsing;

/// <summary>
/// Represents a namespace in C++.
/// </summary>
internal sealed class Namespace : IContainerUnit
{
	/// <inheritdoc/>
	public string Name { get; }

	/// <inheritdoc/>
	public ImmutableArray<Variable> Fields { get; }
	/// <inheritdoc/>
	public ImmutableArray<Method> Methods { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="Namespace"/>.
	/// </summary>
	/// <param name="name">The name of the class.</param>
	/// <param name="fields">All of the fields that are contained.</param>
	/// <param name="methods">All of the methods that are contained.</param>
	private Namespace( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		Name = name;

		Fields = fields;
		Methods = methods;
	}

	/// <summary>
	/// Returns a new instance of the <see cref="Namespace"/> with the fields given.
	/// </summary>
	/// <param name="fields">The new fields to place in the instance.</param>
	/// <returns>A new instance of the <see cref="ClNamespaceass"/> with the fields given.</returns>
	internal Namespace WithFields( in ImmutableArray<Variable> fields )
	{
		return new Namespace( Name, fields, Methods );
	}

	/// <summary>
	/// Returns a new instance of the <see cref="Namespace"/> with the methods given.
	/// </summary>
	/// <param name="methods">The new methods to place in the instance.</param>
	/// <returns>A new instance of the <see cref="Namespace"/> with the methods given.</returns>
	internal Namespace WithMethods( in ImmutableArray<Method> methods )
	{
		return new Namespace( Name, Fields, methods );
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return Name;
	}

	/// <inheritdoc/>
	IContainerUnit IContainerUnit.WithFields( in ImmutableArray<Variable> fields ) => WithFields( fields );
	/// <inheritdoc/>
	IContainerUnit IContainerUnit.WithMethods( in ImmutableArray<Method> methods ) => WithMethods( methods );

	/// <summary>
	/// Returns a new instance of <see cref="Namespace"/>.
	/// </summary>
	/// <param name="name">The name of the namespace.</param>
	/// <param name="fields">The fields contained in the class.</param>
	/// <param name="methods">The methods contained in the class.</param>
	/// <returns>A new instance of <see cref="Namespace"/>.</returns>
	internal static Namespace Create( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		return new Namespace( name, fields, methods );
	}
}
