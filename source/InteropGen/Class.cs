namespace Mocha.InteropGen;

public struct Class
{
	public bool IsStatic { get; set; }
	public string Name { get; set; }
	public List<Function> Functions { get; set; }
}
