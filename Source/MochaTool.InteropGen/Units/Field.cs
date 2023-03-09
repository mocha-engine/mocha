namespace MochaTool.InteropGen;

public struct Variable
{
	public string Name { get; }
	public string Type { get; }

	public Variable( string name, string type )
	{
		Name = name;
		Type = type;
	}

	public override string ToString()
	{
		return $"{Type} {Name}";
	}
}
