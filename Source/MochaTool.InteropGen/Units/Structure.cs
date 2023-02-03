namespace MochaTool.InteropGen;

public struct Structure : IUnit
{
	public Structure( string name ) : this()
	{
		Name = name;

		Fields = new();
		Methods = new();
	}

	public string Name { get; set; }
	public List<Method> Methods { get; set; }
	public List<Variable> Fields { get; set; }

	public override string ToString()
	{
		return Name;
	}
}
