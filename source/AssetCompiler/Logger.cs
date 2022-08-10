namespace AssetCompiler;

public class Logger
{
	private void Log( string prefix, string message ) => Console.WriteLine( $"{"[" + prefix.ToUpper() + "]",-16}{message}" );
	public void Skip( string path ) => Log( "Skip", $"Skipping '{path}' as it matches compiled version" );
	public void UnknownType( string path ) => Log( "Skip", $"Don't know what '{path}' is so not touching it" );
	public void Compiled( string path ) => Log( "OK", $"Compiled '{path}'" );

	public void Processing( string type, string path ) => Log( "PROCESS", $"Processing '{path}' as {type}" );
}
