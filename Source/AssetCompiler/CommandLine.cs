using CommandLine;

namespace MochaTool.AssetCompiler;
public class Options
{
	[Option( 'v', "verbose", Required = false, HelpText = "Set output to verbose messages." )]
	public bool Verbose { get; set; }

	[Option( 'f', "force-recompile", Required = false, HelpText = "Force re-compile all assets (slow!)" )]
	public bool ForceRecompile { get; set; }

	[Option( "mountpoints", Required = true, HelpText = "Which file or folder should we compile?" )]
	public IEnumerable<string> MountPoints { get; set; }
}

public static class CommandLine
{
	static void Main( string[] args )
	{
		var offlineCompiler = new OfflineAssetCompiler();
		Parser.Default.ParseArguments<Options>( args ).WithParsed( offlineCompiler.Run );
	}
}
