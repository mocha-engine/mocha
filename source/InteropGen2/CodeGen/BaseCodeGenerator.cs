internal class BaseCodeGenerator
{
	protected List<IUnit> Units { get; } = new();

	public BaseCodeGenerator( List<IUnit> units )
	{
		Units = units;
	}

	protected string GetHeader()
	{
		return "/*\r\n" +
			" * Generated using InteropGen 2\r\n" +
			$" * on {DateTime.Now}\r\n" +
			" */";
	}
}
