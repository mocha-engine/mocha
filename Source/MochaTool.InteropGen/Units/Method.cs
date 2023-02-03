namespace MochaTool.InteropGen;

public struct Method
{
	public Method( string name, string returnType )
	{
		Name = name;
		ReturnType = returnType;
		Parameters = new();
	}

	public bool IsConstructor { get; set; } = false;
	public bool IsDestructor { get; set; } = false;
	public bool IsStatic { get; set; } = false;

	public string Name { get; set; }
	public string ReturnType { get; set; }
	public List<Variable> Parameters { get; set; }

	public override string ToString()
	{
		var p = string.Join( ", ", Parameters );
		return $"{ReturnType} {Name}( {p} )";
	}
}
