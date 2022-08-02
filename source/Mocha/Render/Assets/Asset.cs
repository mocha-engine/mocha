namespace Mocha.Renderer;

public class Asset
{
	public string Path { get; set; } = $"Asset {All.Count}";
	public static List<Asset> All { get; private set; } = new();
}
