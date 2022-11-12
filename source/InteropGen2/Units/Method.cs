public struct Method
{
	public Method( string name, string returnType )
	{
		Name = name;
		ReturnType = Utils.CppTypeToCsharp( returnType );
		Parameters = new();

		IsStatic = false;
	}

	public string Name { get; set; }
	public string ReturnType { get; set; }
	public bool IsStatic { get; set; }
	public List<Variable> Parameters { get; set; }

	public override string ToString()
	{
		var p = string.Join( ", ", Parameters );
		return $"{ReturnType} {Name}( {p} )";
	}
}
