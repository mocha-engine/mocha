using System.Text;

namespace Mocha.Common;

internal class MountedFileSystem
{
	public string MountPoint { get; private set; }

	public MountedFileSystem( string mountPoint )
	{
		MountPoint = Path.GetFullPath( mountPoint );
	}

	public string GetAbsolutePath( string relativePath )
	{
		var combinedPath = Path.Combine( MountPoint, relativePath );
		return Path.GetFullPath( combinedPath ).NormalizePath();
	}

	public string GetRelativePath( string absolutePath )
	{
		return Path.GetRelativePath( MountPoint, absolutePath ).NormalizePath();
	}
}

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

public class FileSystem
{
	public static FileSystem Mounted { get; set; }

	public IAssetCompiler? AssetCompiler { get; set; }

	private List<FileSystemWatcher> _watchers = new();
	private List<MountedFileSystem> _mountedFileSystems = new();

	public FileSystem( params string[] mountPoints )
	{
		foreach ( var mountPoint in mountPoints )
		{
			_mountedFileSystems.Add( new MountedFileSystem( mountPoint ) );
		}
	}

	private bool TryFindFile( string relativePath, out string? outPath, FileSystemOptions? options = null )
	{
		foreach ( var mountedFs in _mountedFileSystems )
		{
			var path = mountedFs.GetAbsolutePath( relativePath );

			if ( Path.Exists( path ) )
			{
				outPath = path;
				return true;
			}
		}

		outPath = null;
		return false;
	}

	public bool TryGetRelativePath( string absolutePath, out string? outPath, FileSystemOptions? options = null )
	{
		foreach ( var mountedFs in _mountedFileSystems )
		{
			var path = mountedFs.GetRelativePath( absolutePath );

			if ( Path.Exists( path ) )
			{
				outPath = path;
				return true;
			}
		}

		outPath = null;
		return false;
	}

	public string GetAbsolutePath( string relativePath, bool ignorePathNotFound = false, FileSystemOptions? options = null )
	{
		options ??= FileSystemOptions.Default;

		bool isResource = false;
		ResourceType resourceType = ResourceType.Default;

		if ( !options.Value.DisableAutoCompile )
		{
			var extension = Path.GetExtension( relativePath );
			var matchingType = ResourceType.GetResourceForExtension( extension );

			//
			// Check if this path has a registered file type, compile it if it does
			//
			if ( matchingType.HasValue )
			{
				// This is a resource so we'll add the compiled extension to it
				relativePath += "_c";

				isResource = true;
				resourceType = matchingType.Value;
			}
		}

		if ( TryFindFile( relativePath, out var locatedPath ) )
		{
			// Try to find either the path provided OR the compiled file
			// if this is a resource type

			return locatedPath!;
		}
		else if ( isResource )
		{
			// The file doesn't exist... but it's a resource. Chances are
			// we're looking at an uncompiled file. Try to compile it.

			// Try to locate a potential source asset for the resource we're trying to find
			foreach ( var sourceExtension in resourceType.SourceExtensions )
			{
				var searchPath = Path.ChangeExtension( relativePath, sourceExtension );

				if ( TryFindFile( searchPath, out var sourcePath ) )
				{
					// We found a source file, let's compile that and use it
					AssetCompiler?.CompileFile( sourcePath! );

					// Compile complete so let's return the compiled file
					if ( TryFindFile( relativePath, out var compiledResultPath ) )
						return compiledResultPath!;
				}
			}
		}

		// Default to using the first mounted file system
		return _mountedFileSystems[0].GetAbsolutePath( relativePath ); ;
	}

	public FileStream OpenRead( string relativePath, FileSystemOptions? options = null )
	{
		var absolutePath = GetAbsolutePath( relativePath, options: options );
		return new FileStream( absolutePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
	}

	public FileStream OpenWrite( string relativePath, FileSystemOptions? options = null )
	{
		return new FileStream( GetAbsolutePath( relativePath, ignorePathNotFound: true, options: options ), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite );
	}

	public byte[] ReadAllBytes( string relativePath, FileSystemOptions? options = null )
	{
		using ( var fileStream = OpenRead( relativePath, options ) )
		{
			var buffer = new byte[fileStream.Length];
			fileStream.Read( buffer, 0, buffer.Length );
			return buffer;
		}
	}

	public string ReadAllText( string relativePath, FileSystemOptions? options = null )
	{
		var bytes = ReadAllBytes( relativePath, options );
		return Encoding.Default.GetString( bytes );
	}

	public Task<byte[]> ReadAllBytesAsync( string relativePath, FileSystemOptions? options = null )
	{
		return Task.Run( () => ReadAllBytes( relativePath, options ) );
	}

	public Task<string> ReadAllTextAsync( string relativePath, FileSystemOptions? options = null )
	{
		return Task.Run( () => ReadAllText( relativePath, options ) );
	}

	public void WriteAllBytes( string relativePath, byte[] data, FileSystemOptions? options = null )
	{
		using ( var fileStream = OpenWrite( relativePath, options ) )
		{
			fileStream.Write( data, 0, data.Length );
		}
	}

	public void WriteAllText( string relativePath, string data, FileSystemOptions? options = null )
	{
		var bytes = Encoding.Default.GetBytes( data );
		WriteAllBytes( relativePath, bytes, options );
	}

	public void WriteAllText( string relativePath, string data, Encoding encoding, FileSystemOptions? options = null )
	{
		var bytes = encoding.GetBytes( data );
		WriteAllBytes( relativePath, bytes, options );
	}

	public bool Exists( string relativePath, FileSystemOptions? options = null )
	{
		return File.Exists( GetAbsolutePath( relativePath, ignorePathNotFound: true, options ) );
	}

	public DateTime GetLastWriteTime( string relativePath, FileSystemOptions? options = null )
	{
		return File.GetLastWriteTime( GetAbsolutePath( relativePath, options: options ) );
	}

	public void Delete( string relativePath, FileSystemOptions? options = null )
	{
		File.Delete( GetAbsolutePath( relativePath, options: options ) );
	}

	public FileSystemWatcher CreateWatcher( string relativeDir, string filter, Action<string?> onChange, NotifyFilters? filters = null )
	{
		var directoryName = GetAbsolutePath( relativeDir );
		var watcher = new FileSystemWatcher( directoryName, filter );

		watcher.IncludeSubdirectories = true;
		watcher.NotifyFilter = filters ?? (NotifyFilters.Attributes
							 | NotifyFilters.CreationTime
							 | NotifyFilters.DirectoryName
							 | NotifyFilters.FileName
							 | NotifyFilters.LastAccess
							 | NotifyFilters.LastWrite
							 | NotifyFilters.Security
							 | NotifyFilters.Size);

		watcher.EnableRaisingEvents = true;
		watcher.Changed += ( _, e ) =>
		{
			TryFindFile( e.Name, out var path );

			onChange( path );
		};

		_watchers.Add( watcher );

		return watcher;
	}

	public List<FileSystemWatcher> CreateMountedFileSystemWatchers( string filter, Action<string?> onChange, NotifyFilters? filters = null )
	{
		var watchers = new List<FileSystemWatcher>();

		foreach ( var mountedFs in _mountedFileSystems )
		{
			var watcher = new FileSystemWatcher( mountedFs.MountPoint, filter );
			watcher.IncludeSubdirectories = true;
			watcher.NotifyFilter = filters ?? (NotifyFilters.Attributes
								 | NotifyFilters.CreationTime
								 | NotifyFilters.DirectoryName
								 | NotifyFilters.FileName
								 | NotifyFilters.LastAccess
								 | NotifyFilters.LastWrite
								 | NotifyFilters.Security
								 | NotifyFilters.Size);

			watcher.EnableRaisingEvents = true;
			watcher.Changed += ( _, e ) =>
			{
				var path = mountedFs.GetAbsolutePath( e.Name );
				onChange( path );
			};

			watchers.Add( watcher );
		}

		return watchers;
	}

	public bool IsFileReady( string relativePath, FileSystemOptions? options = null )
	{
		var path = GetAbsolutePath( relativePath, options: options );

		if ( !Path.Exists( path ) )
		{
			throw new Exception( "File does not exist" );
		}

		try
		{
			using FileStream inputStream = File.Open( path, FileMode.Open, FileAccess.Read, FileShare.None );
			return inputStream.Length > 0;
		}
		catch ( Exception )
		{
			return false;
		}
	}

	public string GetFullPath( string filePath, FileSystemOptions? options = null )
	{
		TryFindFile( filePath, out var path, options );
		return path;
	}

	private IEnumerable<(FileAttributes Attributes, string AbsolutePath)> GetEntries( string directory, FileSystemOptions? options = null )
	{
		List<(FileAttributes Attributes, string AbsolutePath)> entries = new();

		foreach ( var mountedFs in _mountedFileSystems )
		{
			var mountedDirectory = mountedFs.GetAbsolutePath( directory );

			if ( !Directory.Exists( mountedDirectory ) )
				continue;

			var fsEntries = Directory.GetFileSystemEntries( mountedDirectory );

			// Select relative paths
			entries.AddRange( fsEntries.Select( x => (File.GetAttributes( x ), x.NormalizePath()) ) );
		}

		// Return paths without duplicates
		return entries.Distinct();
	}

	public IEnumerable<string> GetFilesAbsolute( string directory, FileSystemOptions? options = null )
	{
		return GetEntries( directory, options )
			.Where( x => !x.Attributes.HasFlag( FileAttributes.Directory ) )
			.Select( x => x.AbsolutePath );
	}

	public IEnumerable<string> GetFiles( string directory, FileSystemOptions? options = null )
	{
		return GetFilesAbsolute( directory, options )
			.Select( x => GetRelativePath( x ) );
	}

	public IEnumerable<string> GetDirectoriesAbsolute( string directory, FileSystemOptions? options = null )
	{
		return GetEntries( directory, options )
			.Where( x => x.Attributes.HasFlag( FileAttributes.Directory ) )
			.Select( x => x.AbsolutePath );
	}

	public IEnumerable<string> GetDirectories( string directory, FileSystemOptions? options = null )
	{
		return GetDirectoriesAbsolute( directory, options ).Select( x => GetRelativePath( x ) );
	}

	public string GetRelativePath( string filePath, FileSystemOptions? options = null )
	{
		TryFindFile( filePath, out var path, options );
		return path;
	}
}
