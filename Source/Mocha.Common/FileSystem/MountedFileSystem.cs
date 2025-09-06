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
