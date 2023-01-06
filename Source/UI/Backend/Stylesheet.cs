namespace Mocha.UI;

public class Rule
{
	public List<Selector> Selectors { get; set; }
	public StyleValues StyleValues { get; set; }
}

public class Selector
{
	public string TagName { get; set; }
	public string Id { get; set; }
	public List<string> Class { get; set; }
	public string PseudoClass { get; set; }
}

public class Stylesheet
{
	public List<Rule> Rules { get; set; }
}
