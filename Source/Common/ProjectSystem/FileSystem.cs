using Mocha.AssetCompiler;
using System.Text.Json;

namespace Mocha.Common;

public class FileSystem
{
	public static FileSystem Game { get; set; }
	private List<FileSystemWatcher> Watchers { get; } = new();

	private string BasePath { get; }
	public IAssetCompiler AssetCompiler { get; set; }

	public FileSystem( string relativePath )
	{
		this.BasePath = Path.GetFullPath( relativePath, Directory.GetCurrentDirectory() );
	}

	public string GetAbsolutePath( string relativePath, bool ignorePathNotFound = false, bool ignoreCompiledFiles = false )
	{
		string path = Path.Combine( BasePath, relativePath ).NormalizePath();

		if ( !ignoreCompiledFiles )
		{
			var extension = Path.GetExtension( relativePath );
			var matchingType = FileType.GetFileTypeForExtension( extension );

			//
			// Check if this path has a registered file type, compile it if it does
			//
			if ( matchingType.HasValue )
			{
				var type = matchingType.Value;

				// This is a mocha file so we'll add the compiled extension to it
				path += "_c";

				//
				// Try to locate a potential source asset for the resource we're trying to find
				//
				string sourcePath = path;
				foreach ( var sourceExtension in type.SourceExtensions )
				{
					var lookPath = Path.ChangeExtension( sourcePath, sourceExtension );
					if ( Path.Exists( lookPath ) )
					{
						sourcePath = lookPath;
						break;
					}
				}

				//
				// Did we find a valid source path for this file?
				//
				if ( sourcePath != path )
				{
					// Compile asset if needed
					if ( File.Exists( sourcePath ) )
					{
						AssetCompiler.CompileFile( sourcePath );
					}
				}
			}
		}

		// Check if path exists (we do this after compiling the asset in case
		// asset compilation creates the file)
		if ( !File.Exists( path ) && !Directory.Exists( path ) && !ignorePathNotFound )
		{
			Log.Warning( $"Path not found: {path}. Continuing anyway." );
		}

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
		return File.Exists( GetAbsolutePath( relativePath, ignorePathNotFound: true ) );
	}

	public FileSystemWatcher CreateWatcher( string relativeDir, string filter, Action onChange )
	{
		var directoryName = GetAbsolutePath( relativeDir );
		var watcher = new FileSystemWatcher( directoryName, filter );

		watcher.IncludeSubdirectories = true;
		watcher.NotifyFilter = NotifyFilters.Attributes
							 | NotifyFilters.CreationTime
							 | NotifyFilters.DirectoryName
							 | NotifyFilters.FileName
							 | NotifyFilters.LastAccess
							 | NotifyFilters.LastWrite
							 | NotifyFilters.Security
							 | NotifyFilters.Size;

		watcher.EnableRaisingEvents = true;
		watcher.Changed += ( _, _ ) => onChange();

		Watchers.Add( watcher );

		return watcher;
	}

	public bool IsFileReady( string path )
	{
		try
		{
			using FileStream inputStream = File.Open( GetAbsolutePath( path ), FileMode.Open, FileAccess.Read, FileShare.None );
			return inputStream.Length > 0;
		}
		catch ( Exception )
		{
			return false;
		}
	}

	public string GetFullPath( string filePath )
	{
		return Path.Combine( BasePath, filePath );
	}

	public IEnumerable<string> GetFiles( string directory )
	{
		return Directory.GetFiles( GetAbsolutePath( directory ) );
	}

	public IEnumerable<string> GetDirectories( string directory )
	{
		return Directory.GetDirectories( GetAbsolutePath( directory ) );
	}

	public string GetRelativePath( string filePath )
	{
		return Path.GetRelativePath( BasePath, filePath );
	}

	public T? Deserialize<T>( string filePath )
	{
		var text = ReadAllText( filePath );
		return JsonSerializer.Deserialize<T>( text );
	}

	public IEnumerable<string> FindAllFiles( string filter )
	{
		return Directory.GetFiles( BasePath, filter, SearchOption.AllDirectories );
	}
}
