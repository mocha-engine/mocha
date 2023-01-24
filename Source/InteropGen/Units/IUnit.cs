namespace MochaTool.InteropGen;

public interface IUnit
{
	public string Name { get; set; }
	public List<Variable> Fields { get; set; }
	public List<Method> Methods { get; set; }
}
