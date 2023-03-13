namespace MochaTool.InteropGen;

internal sealed class Variable
{
	internal string Name { get; }
	internal string Type { get; }

	internal Variable( string name, string type )
	{
		Name = name;
		Type = type;
	}

	public override string ToString()
	{
		return $"{Type} {Name}";
	}
}
