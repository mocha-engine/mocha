namespace AssetCompiler;

public class Logger
{
	private int _successCount = 0;
	private int _skipCount = 0;
	private int _failCount = 0;
	private int _upToDateCount = 0;

	private void Log( string prefix, string message )
	{
		Console.WriteLine( $"{"[" + prefix.ToUpper() + "]",-16}{message}" );
	}

	public void UpToDate( string path )
	{
		_upToDateCount++;
		Log( "Up-to-date", $"'{path}' is up to date, skipping" );
	}

	public void UnknownType( string path )
	{
		_skipCount++;
		Log( "Skip", $"'{path}' has an unknown resource type" );
	}

	public void Compiled( string path )
	{
		_successCount++;
		Log( "Success", $"'{path}' compiled" );
	}

	public void Fail( string path, Exception? e = null )
	{
		_failCount++;
		Log( "Fail", $"{path} failed to compile. Error was: {e?.Message ?? "Unknown."}" );
		if ( e.StackTrace is not null )
			Log( "StackTrace", e.StackTrace );
	}

	public void Processing( string type, string path )
	{
		Log( "Process", $"'{path}' is a {type}" );
	}

	public void Results( TimeSpan totalTime )
	{
		Log( "Results", $"========== Build: " +
			$"{_successCount} succeeded, " +
			$"{_failCount} failed, " +
			$"{_upToDateCount} up-to-date, " +
			$"{_skipCount} skipped " +
			$"==========\n" +
			$"Build took {totalTime.TotalSeconds} seconds." );
	}
}
