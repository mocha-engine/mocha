using System.Collections.Immutable;

namespace MochaTool.InteropGen;

public sealed class Method
{
	public string Name { get; }
	public string ReturnType { get; }

	public bool IsConstructor { get; } = false;
	public bool IsDestructor { get; } = false;
	public bool IsStatic { get; } = false;

	public ImmutableArray<Variable> Parameters { get; }

	public Method( string name, string returnType, bool isConstructor, bool isDestructor, bool isStatic, in ImmutableArray<Variable> parameters )
	{
		Name = name;
		ReturnType = returnType;

		IsConstructor = isConstructor;
		IsDestructor = isDestructor;
		IsStatic = isStatic;

		Parameters = parameters;
	}
	
	public Method WithParameters( in ImmutableArray<Variable> parameters )
	{
		return new( Name, ReturnType, IsConstructor, IsDestructor, IsStatic, parameters );
	}

	public override string ToString()
	{
		var p = string.Join( ", ", Parameters );
		return $"{ReturnType} {Name}( {p} )";
	}

	public static Method NewConstructor( string name, string returnType, in ImmutableArray<Variable> parameters )
	{
		return new( name, returnType, true, false, false, parameters );
	}

	public static Method NewDestructor( string name, string returnType, in ImmutableArray<Variable> parameters )
	{
		return new( name, returnType, false, true, false, parameters );
	}

	public static Method NewMethod( string name, string returnType, bool isStatic, in ImmutableArray<Variable> parameters )
	{
		return new( name, returnType, false, false, isStatic, parameters );
	}
}
