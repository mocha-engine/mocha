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
		bool isAsset = false;
		string path = Path.Combine( this.BasePath, relativePath ).NormalizePath();
		string sourcePath = path;

		if ( !ignoreCompiledFiles )
		{
			switch ( Path.GetExtension( relativePath ) )
			{
				case ".mmdl":
				case ".mmat":
				case ".mtex":
				case ".mfnt":
					// Load compiled assets
					isAsset = true;
					path += "_c";
					break;
			}

			// HACK: Is this an image? If so, check for a source .ttf, .jpg, or .png, and set that
			// as the source path.
			if ( Path.GetExtension( relativePath ) == ".mtex" )
			{
				var jpgPath = Path.ChangeExtension( sourcePath, "jpg" );
				var pngPath = Path.ChangeExtension( sourcePath, "png" );
				var ttfPath = Path.ChangeExtension( sourcePath, "ttf" );

				if ( Path.Exists( jpgPath ) )
					sourcePath = jpgPath;
				else if ( Path.Exists( pngPath ) )
					sourcePath = pngPath;
				else if ( Path.Exists( ttfPath ) )
					sourcePath = ttfPath;
			}

			// HACK: Is this a font? 
			if ( Path.GetExtension( relativePath ) == ".mfnt" )
			{
				var ttfPath = Path.ChangeExtension( sourcePath, "ttf" );
				if ( Path.Exists( ttfPath ) )
					sourcePath = ttfPath;
			}

			// Compile asset if needed
			if ( isAsset && File.Exists( sourcePath ) )
			{
				AssetCompiler.CompileFile( sourcePath );
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
