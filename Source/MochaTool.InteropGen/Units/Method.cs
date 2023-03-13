using System.Collections.Immutable;

namespace MochaTool.InteropGen;

internal sealed class Method
{
	internal string Name { get; }
	internal string ReturnType { get; }

	internal bool IsConstructor { get; } = false;
	internal bool IsDestructor { get; } = false;
	internal bool IsStatic { get; } = false;

	internal ImmutableArray<Variable> Parameters { get; }

	private Method( string name, string returnType, bool isConstructor, bool isDestructor, bool isStatic, in ImmutableArray<Variable> parameters )
	{
		Name = name;
		ReturnType = returnType;

		IsConstructor = isConstructor;
		IsDestructor = isDestructor;
		IsStatic = isStatic;

		Parameters = parameters;
	}
	internal Method WithParameters( in ImmutableArray<Variable> parameters )
	{
		return new( Name, ReturnType, IsConstructor, IsDestructor, IsStatic, parameters );
	}

	public override string ToString()
	{
		var p = string.Join( ", ", Parameters );
		return $"{ReturnType} {Name}( {p} )";
	}

	internal static Method NewConstructor( string name, string returnType, in ImmutableArray<Variable> parameters )
	{
		return new( name, returnType, true, false, false, parameters );
	}

	internal static Method NewDestructor( string name, string returnType, in ImmutableArray<Variable> parameters )
	{
		return new( name, returnType, false, true, false, parameters );
	}

	internal static Method NewMethod( string name, string returnType, bool isStatic, in ImmutableArray<Variable> parameters )
	{
		return new( name, returnType, false, false, isStatic, parameters );
	}
}
