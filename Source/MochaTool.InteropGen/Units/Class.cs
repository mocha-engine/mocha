using System.Collections.Immutable;

namespace MochaTool.InteropGen;

internal sealed class Class : IUnit
{
	public string Name { get; }
	public bool IsNamespace { get; }

	public ImmutableArray<Variable> Fields { get; }
	public ImmutableArray<Method> Methods { get; }

	private Class( string name, bool isNamespace, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		Name = name;
		IsNamespace = isNamespace;

		Fields = fields;
		Methods = methods;
	}

	internal Class WithFields( in ImmutableArray<Variable> fields )
	{
		return new Class( Name, IsNamespace, fields, Methods );
	}

	internal Class WithMethods( in ImmutableArray<Method> methods )
	{
		return new Class( Name, IsNamespace, Fields, methods );
	}

	public override string ToString()
	{
		return Name;
	}

	IUnit IUnit.WithFields( in ImmutableArray<Variable> fields ) => WithFields( fields );
	IUnit IUnit.WithMethods( in ImmutableArray<Method> methods ) => WithMethods( methods );

	internal static Class NewClass( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		return new Class( name, false, fields, methods );
	}

	internal static Class NewNamespace( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		return new Class( name, true, fields, methods );
	}
}
