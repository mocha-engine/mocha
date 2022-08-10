namespace Mocha.Common;

public class FileSystem
{

#if DEBUG
	public static FileSystem Game => new FileSystem( "..\\content" );
#else
	public static FileSystem Game => new FileSystem( "content" );
#endif

	private string BasePath { get; }

	public FileSystem( string relativePath )
	{
		this.BasePath = Path.GetFullPath( relativePath, Directory.GetCurrentDirectory() );
	}

	private string GetAbsolutePath( string relativePath )
	{
		var path = Path.Combine( this.BasePath, relativePath ).NormalizePath();

		if ( !File.Exists( path ) && !Directory.Exists( path ) )
			Log.Error( $"File not found: {path}" );

		return path;
	}

	public string ReadAllText( string relativePath )
	{
		return File.ReadAllText( GetAbsolutePath( relativePath ) );
	}

	public byte[] ReadAllBytes( string relativePath )
	{
		return File.ReadAllBytes( GetAbsolutePath( relativePath ) );
	}

	public FileStream OpenRead( string relativePath )
	{
		return File.OpenRead( GetAbsolutePath( relativePath ) );
	}

	public bool Exists( string relativePath )
	{
		return File.Exists( GetAbsolutePath( relativePath ) );
	}

	public FileSystemWatcher CreateWatcher( string relativeDir, string filter )
	{
		var directoryName = GetAbsolutePath( relativeDir );
		var watcher = new FileSystemWatcher( directoryName, filter );

		watcher.NotifyFilter = NotifyFilters.Attributes
							 | NotifyFilters.CreationTime
							 | NotifyFilters.DirectoryName
							 | NotifyFilters.FileName
							 | NotifyFilters.LastAccess
							 | NotifyFilters.LastWrite
							 | NotifyFilters.Security
							 | NotifyFilters.Size;

		watcher.EnableRaisingEvents = true;

		return watcher;
	}
}
