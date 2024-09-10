namespace MochaTool.InteropGen.Parsing;

/// <summary>
/// Represents a variable in C++. This can be a field, parameter, etc.
/// </summary>
internal sealed class Variable : IUnit
{
	/// <summary>
	/// The name of the variable.
	/// </summary>
	public string Name { get; }
	/// <summary>
	/// The literal string containing the type of the variable.
	/// </summary>
	internal string Type { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="Variable"/>.
	/// </summary>
	/// <param name="name">The name of the variable.</param>
	/// <param name="type">The literal string containing the type of the variable.</param>
	internal Variable( string name, string type )
	{
		Name = name;
		Type = type;
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return $"{Type} {Name}";
	}
}
