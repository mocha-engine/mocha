using System.Collections.Immutable;

namespace MochaTool.InteropGen;

internal sealed class Struct : IUnit
{
	public string Name { get; }

	public ImmutableArray<Variable> Fields { get; }
	public ImmutableArray<Method> Methods { get; }

	private Struct( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		Name = name;

		Fields = fields;
		Methods = methods;
	}

	internal Struct WithFields( in ImmutableArray<Variable> fields )
	{
		return new( Name, fields, Methods );
	}

	internal Struct WithMethods( in ImmutableArray<Method> methods )
	{
		return new( Name, Fields, methods );
	}

	public override string ToString()
	{
		return Name;
	}

	IUnit IUnit.WithFields( in ImmutableArray<Variable> fields ) => WithFields( fields );
	IUnit IUnit.WithMethods( in ImmutableArray<Method> methods ) => WithMethods( methods );

	internal static Struct NewStructure( string name, in ImmutableArray<Variable> fields, in ImmutableArray<Method> methods )
	{
		return new( name, fields, methods );
	}
}
