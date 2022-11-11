struct Field
{
	public Field( string name, string type ) : this()
	{
		Name = name;
		Type = type;
	}

	public string Name { get; set; }
	public string Type { get; set; }

	public override string ToString()
	{
		return $"{Type} {Name}";
	}
}
