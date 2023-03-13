using System.Collections.Immutable;

namespace MochaTool.InteropGen;

/// <summary>
/// Represents a method in C++.
/// </summary>
internal sealed class Method
{
	/// <summary>
	/// The name of the method.
	/// </summary>
	internal string Name { get; }
	/// <summary>
	/// The literal string containing the return type of the method.
	/// </summary>
	internal string ReturnType { get; }

	/// <summary>
	/// Whether or not the method is a constructor.
	/// </summary>
	internal bool IsConstructor { get; } = false;
	/// <summary>
	/// Whether or not the method is a destructor.
	/// </summary>
	internal bool IsDestructor { get; } = false;
	/// <summary>
	/// Whether or not the method is static.
	/// </summary>
	internal bool IsStatic { get; } = false;

	/// <summary>
	/// An array of all the parameters in the method.
	/// </summary>
	internal ImmutableArray<Variable> Parameters { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="Method"/>.
	/// </summary>
	/// <param name="name">The name of the method.</param>
	/// <param name="returnType">The literal string containing the return type of the method.</param>
	/// <param name="isConstructor">Whether or not the method is a constructor.</param>
	/// <param name="isDestructor">Whether or not the method is a destructor.</param>
	/// <param name="isStatic">Whether or not the method is static.</param>
	/// <param name="parameters">An array of all the parameters in the method.</param>
	private Method( string name, string returnType, bool isConstructor, bool isDestructor, bool isStatic, in ImmutableArray<Variable> parameters )
	{
		Name = name;
		ReturnType = returnType;

		IsConstructor = isConstructor;
		IsDestructor = isDestructor;
		IsStatic = isStatic;

		Parameters = parameters;
	}

	/// <summary>
	/// Returns a new instance of the <see cref="Method"/> with the parameters given.
	/// </summary>
	/// <param name="parameters">The new fields to place in the instance.</param>
	/// <returns>A new instance of the <see cref="Method"/> with the parameters given.</returns>
	internal Method WithParameters( in ImmutableArray<Variable> parameters )
	{
		return new( Name, ReturnType, IsConstructor, IsDestructor, IsStatic, parameters );
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		var p = string.Join( ", ", Parameters );
		return $"{ReturnType} {Name}( {p} )";
	}

	/// <summary>
	/// Returns a new instance of <see cref="Method"/> as a constructor.
	/// </summary>
	/// <param name="name">The name of the method.</param>
	/// <param name="returnType">The literal string containing the return type of the method.</param>
	/// <param name="parameters">An array of all the parameters in the method.</param>
	/// <returns>A new instance of <see cref="Method"/> as a constructor.</returns>
	internal static Method NewConstructor( string name, string returnType, in ImmutableArray<Variable> parameters )
	{
		return new( name, returnType, true, false, false, parameters );
	}

	/// <summary>
	/// Returns a new instance of <see cref="Method"/> as a destructor.
	/// </summary>
	/// <param name="name">The name of the method.</param>
	/// <param name="returnType">The literal string containing the return type of the method.</param>
	/// <param name="parameters">An array of all the parameters in the method.</param>
	/// <returns>A new instance of <see cref="Method"/> as a destructor.</returns>
	internal static Method NewDestructor( string name, string returnType, in ImmutableArray<Variable> parameters )
	{
		return new( name, returnType, false, true, false, parameters );
	}

	/// <summary>
	/// Returns a new instance of <see cref="Method"/>.
	/// </summary>
	/// <param name="name">The name of the method.</param>
	/// <param name="returnType">The literal string containing the return type of the method.</param>
	/// <param name="isStatic">Whether or not the method is static.</param>
	/// <param name="parameters">An array of all the parameters in the method.</param>
	/// <returns>A new instance of <see cref="Method"/>.</returns>
	internal static Method NewMethod( string name, string returnType, bool isStatic, in ImmutableArray<Variable> parameters )
	{
		return new( name, returnType, false, false, isStatic, parameters );
	}
}
