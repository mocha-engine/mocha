using System.Text;
using System.Text.Json;

namespace Mocha.Common;

internal class MountedFileSystem
{
	public string MountPoint { get; private set; }

	public MountedFileSystem( string mountPoint )
	{
		MountPoint = mountPoint;
	}

	public string GetAbsolutePath( string relativePath )
	{
		return Path.Combine( MountPoint, relativePath ).NormalizePath();
	}
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

	private bool TryFindFile( string relativePath, out string? outPath )
	{
		Log.Trace( $"Trying to find file '{relativePath}' across mounted file systems..." );

		foreach ( var mountedFs in _mountedFileSystems )
		{
			var path = mountedFs.GetAbsolutePath( relativePath );

			if ( File.Exists( path ) )
			{
				Log.Trace( $"\t- Present at '{path}'" );
				outPath = path;
				return true;
			}

			Log.Trace( $"\t- Does not exist at '{path}'" );
		}

		outPath = null;
		return false;
	}

	public string GetAbsolutePath( string relativePath, bool ignorePathNotFound = false, bool ignoreCompiledFiles = false )
	{
		bool isResource = false;
		ResourceType resourceType = ResourceType.Default;

		if ( !ignoreCompiledFiles )
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

		if ( TryFindFile( relativePath, out var path ) )
		{
			// Try to find either the path provided OR the compiled file
			// if this is a resource type
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
					AssetCompiler?.CompileFile( sourcePath );

					// Compile complete so let's find the compiled file...
					TryFindFile( relativePath, out path );

					break;
				}
			}
		}
		else
		{
			// We couldn't find the source file AND this wasn't a resource
			// We'll handle that in the next step
		}

		// Check if path exists (we do this after compiling the asset in case
		// asset compilation creates the file)
		if ( !File.Exists( path ) && !Directory.Exists( path ) && !ignorePathNotFound )
		{
			Log.Warning( $"Path not found: '{relativePath}'. Continuing anyway." );
		}

		return path;
	}

	public FileStream OpenRead( string relativePath )
	{
		return new FileStream( GetAbsolutePath( relativePath ), FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
	}

	public byte[] ReadAllBytes( string relativePath )
	{
		using ( var fileStream = OpenRead( relativePath ) )
		{
			var buffer = new byte[fileStream.Length];
			fileStream.Read( buffer, 0, buffer.Length );
			return buffer;
		}
	}

	public string ReadAllText( string relativePath )
	{
		var bytes = ReadAllBytes( relativePath );
		return Encoding.Default.GetString( bytes );
	}

	public bool Exists( string relativePath )
	{
		return File.Exists( GetAbsolutePath( relativePath, ignorePathNotFound: true ) );
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

	public bool IsFileReady( string relativePath )
	{
		var path = GetAbsolutePath( relativePath );

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

	public string GetFullPath( string filePath )
	{
		TryFindFile( filePath, out var path );
		return path;
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
		TryFindFile( filePath, out var path );
		return path;
	}

	public T? Deserialize<T>( string filePath )
	{
		var text = ReadAllText( filePath );
		return JsonSerializer.Deserialize<T>( text );
	}
}
