namespace Mocha.Editor;

public class FileSystem
{
	private Glue.CFileSystem NativeFileSystem { get; }

	public FileSystem()
	{
		var contentDirectory = Directory.GetCurrentDirectory();
#if DEBUG
		// Move into project root dir
		contentDirectory = Path.Combine( contentDirectory, ".." );
#endif
		contentDirectory = Path.Combine( contentDirectory, "content" );

		NativeFileSystem = new( contentDirectory );
	}

	public bool DirectoryExists( string dirPath )
	{
		return NativeFileSystem.DirectoryExists( dirPath );
	}

	public bool FileExists( string filePath )
	{
		return NativeFileSystem.FileExists( filePath );
	}

	public bool IsDirectory( string dirPath )
	{
		return NativeFileSystem.IsDirectory( dirPath );
	}

	public bool IsFile( string filePath )
	{
		return NativeFileSystem.IsFile( filePath );
	}

	public IntPtr GetFiles( string dirPath, IntPtr outSize )
	{
		return NativeFileSystem.GetFiles( dirPath, outSize );
	}

	public IntPtr GetDirectories( string dirPath, IntPtr outSize )
	{
		return NativeFileSystem.GetDirectories( dirPath, outSize );
	}

	public string ReadAllText( string filePath )
	{
		return NativeFileSystem.ReadAllText( filePath );
	}

	public IntPtr ReadAllBytes( string filePath, IntPtr outSize )
	{
		return NativeFileSystem.ReadAllBytes( filePath, outSize );
	}

	public bool WriteAllText( string filePath, string text )
	{
		return NativeFileSystem.WriteAllText( filePath, text );
	}
}
