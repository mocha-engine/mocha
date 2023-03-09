using System.Collections.Immutable;

namespace MochaTool.InteropGen;

public sealed class Struct : IUnit
{
	public string Name { get; }

	public ImmutableArray<Variable> Fields { get; }
	public ImmutableArray<Method> Methods { get; }

	public Struct( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		Name = name;

		Fields = fields;
		Methods = methods;
	}

	public Struct WithFields( in ImmutableArray<Variable> fields )
	{
		return new( Name, fields, Methods );
	}

	public Struct WithMethods( in ImmutableArray<Method> methods )
	{
		return new( Name, Fields, methods );
	}

	public override string ToString()
	{
		return Name;
	}

	IUnit IUnit.WithFields( in ImmutableArray<Variable> fields ) => WithFields( fields );
	IUnit IUnit.WithMethods( in ImmutableArray<Method> methods ) => WithMethods( methods );

	public static Struct NewStructure( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		return new( name, fields, methods );
	}
}
