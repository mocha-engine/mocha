using System.Collections.Immutable;

namespace MochaTool.InteropGen;

public sealed class Structure : IUnit
{
	public string Name { get; }

	public ImmutableArray<Variable> Fields { get; }
	public ImmutableArray<Method> Methods { get; }

	public Structure( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		Name = name;

		Fields = fields;
		Methods = methods;
	}

	public Structure WithFields( in ImmutableArray<Variable> fields )
	{
		return new( Name, fields, Methods );
	}

	public Structure WithMethods( in ImmutableArray<Method> methods )
	{
		return new( Name, Fields, methods );
	}

	public override string ToString()
	{
		return Name;
	}

	IUnit IUnit.WithFields( in ImmutableArray<Variable> fields ) => WithFields( fields );
	IUnit IUnit.WithMethods( in ImmutableArray<Method> methods ) => WithMethods( methods );

	public static Structure NewStructure( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		return new( name, fields, methods );
	}
}
