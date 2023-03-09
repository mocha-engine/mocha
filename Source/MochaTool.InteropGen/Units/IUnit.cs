using System.Collections.Immutable;

namespace MochaTool.InteropGen;

public interface IUnit
{
	string Name { get; }

	ImmutableArray<Variable> Fields { get; }
	ImmutableArray<Method> Methods { get; }

	IUnit WithFields( in ImmutableArray<Variable> fields );
	IUnit WithMethods( in ImmutableArray<Method> methods );
}
