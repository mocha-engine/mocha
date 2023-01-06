namespace AssetCompiler;

public class Logger
{
	private int SuccessCount = 0;
	private int SkipCount = 0;
	private int FailCount = 0;
	private int UpToDateCount = 0;

	private void Log( string prefix, string message ) => Console.WriteLine( $"{"[" + prefix.ToUpper() + "]",-16}{message}" );

	public void UpToDate( string path )
	{
		UpToDateCount++;
		Log( "Up-to-date", $"Not compiling '{path}' as it matches compiled version" );
	}

	public void UnknownType( string path )
	{
		SkipCount++;
		Log( "Skip", $"Don't know what '{path}' is so not touching it" );
	}

	public void Compiled( string path )
	{
		SuccessCount++;
		Log( "Success", $"Compiled '{path}'" );
	}

	public void Fail( string path, Exception? e = null )
	{
		FailCount++;
		Log( "Fail", $"{path} failed to compile. Error was: {e?.Message ?? "Unknown"}" );
	}

	public void Processing( string type, string path ) => Log( "PROCESS", $"Processing '{path}' as {type}" );

	public void Results( TimeSpan totalTime ) => Log( "Results", $"========== Build: {SuccessCount} succeeded, {FailCount} failed, {UpToDateCount} up-to-date, {SkipCount} skipped ==========\nBuild took {totalTime.TotalSeconds} seconds." );
}
