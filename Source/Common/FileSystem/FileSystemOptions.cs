namespace Mocha.Common;

public struct FileSystemOptions
{
	public bool DisableAutoCompile { get; set; }

	public static FileSystemOptions Default = new()
	{
		DisableAutoCompile = false
	};

	public static FileSystemOptions AssetCompiler = new()
	{
		DisableAutoCompile = true
	};
}
